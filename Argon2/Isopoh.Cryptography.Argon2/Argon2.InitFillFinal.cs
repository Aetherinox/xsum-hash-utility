﻿// <copyright file="Argon2.InitFillFinal.cs" company="Isopoh">
// To the extent possible under law, the author(s) have dedicated all copyright
// and related and neighboring rights to this software to the public domain
// worldwide. This software is distributed without any warranty.
// </copyright>

namespace Isopoh.Cryptography.Argon2
{
    using System;
    using System.Linq;
    using System.Threading;
    using Isopoh.Cryptography.Blake2b;
    using Isopoh.Cryptography.SecureArray;

    /// <summary>
    /// Argon2 Hashing of passwords.
    /// </summary>
    public sealed partial class Argon2
    {
        private void Initialize()
        {
            using var blockHash = SecureArray<byte>.Best(PrehashSeedLength, this.config.SecureArrayCall);
            using (var initialHash = this.InitialHash())
            {
                Array.Copy(initialHash.Buffer, blockHash.Buffer, PrehashDigestLength);
            }

            InitialKat(blockHash.Buffer, this);
            this.FillFirstBlocks(blockHash.Buffer);
        }

        private SecureArray<byte> InitialHash()
        {
            var ret = SecureArray<byte>.Best(Blake2B.OutputLength, this.config.SecureArrayCall);
            using var blakeHash =
                Blake2B.Create(
                    new Blake2BConfig
                    {
                        OutputSizeInBytes = PrehashDigestLength,
                        Result64ByteBuffer = ret.Buffer,
                    },
                    this.config.SecureArrayCall);
            var value = new byte[4];
            Store32(value, this.config.Lanes);
            blakeHash.Update(value);
            Store32(value, this.config.HashLength);
            blakeHash.Update(value);
            Store32(value, this.config.MemoryCost);
            blakeHash.Update(value);
            Store32(value, this.config.TimeCost);
            blakeHash.Update(value);
            Store32(value, (uint)this.config.Version);
            blakeHash.Update(value);
            Store32(value, (uint)this.config.Type);
            blakeHash.Update(value);
            Store32(value, this.config.Password?.Length ?? 0);
            blakeHash.Update(value);
            if (this.config.Password != null)
            {
                blakeHash.Update(this.config.Password);
                if (this.config.ClearPassword)
                {
                    SecureArray.Zero(this.config.Password);
                }
            }

            Store32(value, this.config.Salt?.Length ?? 0);
            blakeHash.Update(value);
            if (this.config.Salt != null)
            {
                blakeHash.Update(this.config.Salt);
            }

            Store32(value, this.config.Secret?.Length ?? 0);
            blakeHash.Update(value);
            if (this.config.Secret != null)
            {
                blakeHash.Update(this.config.Secret);
                if (this.config.ClearSecret)
                {
                    SecureArray.Zero(this.config.Secret);
                }
            }

            Store32(value, this.config.AssociatedData?.Length ?? 0);
            blakeHash.Update(value);
            if (this.config.AssociatedData != null)
            {
                blakeHash.Update(this.config.AssociatedData);
            }

            blakeHash.Finish();

            return ret;
        }

        private void FillFirstBlocks(byte[] blockHash)
        {
            using var blockHashBytes = SecureArray<byte>.Best(BlockSize, this.config.SecureArrayCall);
            for (int l = 0; l < this.config.Lanes; ++l)
            {
                Store32(blockHash, PrehashDigestLength, 0);
                Store32(blockHash, PrehashDigestLength + 4, l);
                Blake2BLong(blockHashBytes.Buffer, blockHash, this.config.SecureArrayCall);
                LoadBlock(this.Memory[l * this.LaneBlockCount], blockHashBytes.Buffer);
                Store32(blockHash, PrehashDigestLength, 1);
                Blake2BLong(blockHashBytes.Buffer, blockHash, this.config.SecureArrayCall);
                LoadBlock(this.Memory[(l * this.LaneBlockCount) + 1], blockHashBytes.Buffer);
            }
        }

        private void FillMemoryBlocks()
        {
            if (this.config.Threads > 1)
            {
                WaitHandle[] waitHandles =
                    Enumerable.Range(
                        0,
                        this.config.Threads > this.config.Lanes ? this.config.Lanes : this.config.Threads)
                        .Select(_ => new AutoResetEvent(false))
                        .Cast<WaitHandle>()
                        .ToArray();
                for (int passNumber = 0; passNumber < this.config.TimeCost; ++passNumber)
                {
                    for (int sliceNumber = 0; sliceNumber < SyncPointCount; ++sliceNumber)
                    {
                        int laneNumber = 0;
                        int remaining = this.config.Lanes;
                        for (; laneNumber < waitHandles.Length && laneNumber < this.config.Lanes; ++laneNumber)
                        {
                            ThreadPool.QueueUserWorkItem(
                                (fs) =>
                                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                                    this.FillSegment(((FillState)fs).Position);
                                    ((FillState)fs).Are.Set();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                                },
                                new FillState(new Position { Pass = passNumber, Lane = laneNumber, Slice = sliceNumber, Index = 0 }, (AutoResetEvent)waitHandles[laneNumber]));
                        }

                        while (laneNumber < this.config.Lanes)
                        {
                            int i = WaitHandle.WaitAny(waitHandles);
                            --remaining;
                            ThreadPool.QueueUserWorkItem(
                                (fs) =>
                                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                                    this.FillSegment(((FillState)fs).Position);
                                    ((FillState)fs).Are.Set();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                                },
                                new FillState(new Position { Pass = passNumber, Lane = laneNumber, Slice = sliceNumber, Index = 0 }, (AutoResetEvent)waitHandles[i]));
                            ++laneNumber;
                        }

                        while (remaining > 0)
                        {
                            _ = WaitHandle.WaitAny(waitHandles);
                            --remaining;
                        }
                    }

                    InternalKat(this, passNumber);
                }
            }
            else
            {
                for (int passNumber = 0; passNumber < this.config.TimeCost; ++passNumber)
                {
                    for (int sliceNumber = 0; sliceNumber < SyncPointCount; ++sliceNumber)
                    {
                        for (int laneNumber = 0; laneNumber < this.config.Lanes; ++laneNumber)
                        {
                            this.FillSegment(
                                new Position
                                {
                                    Pass = passNumber,
                                    Lane = laneNumber,
                                    Slice = sliceNumber,
                                    Index = 0,
                                });
                        }
                    }

                    InternalKat(this, passNumber);
                }
            }
        }

        private SecureArray<byte> Final()
        {
            using var blockHashBuffer = SecureArray<ulong>.Best(BlockSize / 8, this.config.SecureArrayCall);
            var blockHash = new BlockValues(blockHashBuffer.Buffer, 0);
            blockHash.Copy(this.Memory[this.LaneBlockCount - 1]);

            // XOR last blocks
            for (int l = 1; l < this.config.Lanes; ++l)
            {
                blockHash.Xor(this.Memory[(l * this.LaneBlockCount) + (this.LaneBlockCount - 1)]);
            }

            using var blockHashBytes = SecureArray<byte>.Best(BlockSize, this.config.SecureArrayCall);
            StoreBlock(blockHashBytes.Buffer, blockHash);
            var ret = SecureArray<byte>.Best(this.config.HashLength, this.config.SecureArrayCall);
            Blake2BLong(ret.Buffer, blockHashBytes.Buffer, this.config.SecureArrayCall);
            PrintTag(ret.Buffer);
            return ret;
        }

        private void FillSegment(Position position)
        {
            bool dataIndependentAddressing = this.config.Type == Argon2Type.DataIndependentAddressing ||
                                             (this.config.Type == Argon2Type.HybridAddressing && position.Pass == 0 &&
                                              position.Slice < SyncPointCount / 2);
            var pseudoRands = new ulong[this.SegmentBlockCount];
            if (dataIndependentAddressing)
            {
                this.GenerateAddresses(position, pseudoRands);
            }

            // 2 if already generated the first two blocks
            int startingIndex = position.Pass == 0 && position.Slice == 0 ? 2 : 0;
            int curOffset = (position.Lane * this.LaneBlockCount) + (position.Slice * this.SegmentBlockCount) + startingIndex;
            int prevOffset = curOffset % this.LaneBlockCount == 0 ? curOffset + this.LaneBlockCount - 1 : curOffset - 1;

            for (int i = startingIndex; i < this.SegmentBlockCount; ++i, ++curOffset, ++prevOffset)
            {
                if (curOffset % this.LaneBlockCount == 1)
                {
                    prevOffset = curOffset - 1;
                }

                // compute index of reference block taking pseudo-random value from previous block
                ulong pseudoRand = dataIndependentAddressing ? pseudoRands[i] : this.Memory[prevOffset][0];

                // cannot reference other lanes until pass or slice are not zero
                int refLane =
                    (position.Pass == 0 && position.Slice == 0)
                    ? position.Lane
                    : (int)((uint)(pseudoRand >> 32) % (uint)this.config.Lanes);

                // compute possible number of reference blocks in lane
                position.Index = i;
                int refIndex = this.IndexAlpha(position, (uint)pseudoRand, refLane == position.Lane);

                BlockValues refBlock = this.Memory[(this.LaneBlockCount * refLane) + refIndex];
                BlockValues curBlock = this.Memory[curOffset];
                if (this.config.Version == Argon2Version.Sixteen)
                {
                    // version 1.2.1 and earlier: overwrite, not XOR
                    FillBlock(this.Memory[prevOffset], refBlock, curBlock);
                }
                else if (position.Pass == 0)
                {
                    FillBlock(this.Memory[prevOffset], refBlock, curBlock);
                }
                else
                {
                    FillBlockWithXor(this.Memory[prevOffset], refBlock, curBlock);
                }
            }
        }

        private int IndexAlpha(Position position, uint pseudoRand, bool sameLane)
        {
            // Pass 0:
            //   This lane : all already finished segments plus already constructed
            //   blocks in this segment
            // Other lanes : all already finished segments
            // Pass 1+:
            //   This lane : (SYNC_POINTS - 1) last segments plus already constructed
            //   blocks in this segment
            //   Other lanes : (SYNC_POINTS - 1) last segments
            int referenceAreaSize;
            if (position.Pass == 0)
            {
                // first pass
                if (position.Slice == 0)
                {
                    // first slice
                    referenceAreaSize = position.Index - 1; // all but previous
                }
                else
                {
                    if (sameLane)
                    {
                        // same lane, add current segment
                        referenceAreaSize = (position.Slice * this.SegmentBlockCount) + position.Index - 1;
                    }
                    else
                    {
                        referenceAreaSize = (position.Slice * this.SegmentBlockCount) + (position.Index == 0 ? -1 : 0);
                    }
                }
            }
            else
            {
                // second pass
                if (sameLane)
                {
                    referenceAreaSize = this.LaneBlockCount - this.SegmentBlockCount + position.Index - 1;
                }
                else
                {
                    referenceAreaSize = this.LaneBlockCount - this.SegmentBlockCount + (position.Index == 0 ? -1 : 0);
                }
            }

            ulong relativePosition = pseudoRand;
            relativePosition = (relativePosition * relativePosition) >> 32;
            relativePosition = (uint)referenceAreaSize - 1 - (((uint)referenceAreaSize * relativePosition) >> 32);

            int startPosition = position.Pass != 0
                                    ? position.Slice == (SyncPointCount - 1)
                                          ? 0
                                          : (position.Slice + 1) * this.SegmentBlockCount
                                    : 0;
            int absolutePosition = (int)(((ulong)startPosition + relativePosition) % (ulong)this.LaneBlockCount);
            return absolutePosition;
        }

        private void GenerateAddresses(Position position, ulong[] pseudoRands)
        {
            var buf = new ulong[QwordsInBlock * 4];
            var zeroBlock = new BlockValues(buf, 0);
            var inputBlock = new BlockValues(buf, 1);
            var addressBlock = new BlockValues(buf, 2);
            var tmpBlock = new BlockValues(buf, 3);

            inputBlock[0] = (ulong)position.Pass;
            inputBlock[1] = (ulong)position.Lane;
            inputBlock[2] = (ulong)position.Slice;
            inputBlock[3] = (ulong)this.MemoryBlockCount;
            inputBlock[4] = (ulong)this.config.TimeCost;
            inputBlock[5] = (ulong)this.config.Type;
            for (int i = 0; i < this.SegmentBlockCount; ++i)
            {
                if (i % QwordsInBlock == 0)
                {
                    inputBlock[6] += 1;
                    tmpBlock.Init(0);
                    addressBlock.Init(0);
                    FillBlockWithXor(zeroBlock, inputBlock, tmpBlock);
                    FillBlockWithXor(zeroBlock, tmpBlock, addressBlock);
                }

                pseudoRands[i] = addressBlock[i % QwordsInBlock];
            }
        }

        private sealed class Position
        {
            public int Pass { get; init; }

            public int Lane { get; init; }

            public int Slice { get; init; }

            public int Index { get; set; }
        }

        private sealed class FillState
        {
            public FillState(Position position, AutoResetEvent are) => (this.Position, this.Are) = (position, are);

            public Position Position { get; }

            public AutoResetEvent Are { get; }
        }
    }
}
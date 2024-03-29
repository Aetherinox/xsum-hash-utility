﻿// BLAKE2 reference source code package - C# implementation

// Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>
// Modified in 2016 by Michael Heyman for sensitive information

// To the extent possible under law, the author(s) have dedicated all copyright
// and related and neighboring rights to this software to the public domain
// worldwide. This software is distributed without any warranty.

// You should have received a copy of the CC0 Public Domain Dedication along with
// this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
namespace Isopoh.Cryptography.Blake2b
{
    using System;
    using Isopoh.Cryptography.SecureArray;

    /// <summary>
    /// Init/Update/Final for Blake2 hash.
    /// </summary>
    internal class Blake2BHasher : Hasher
    {
        private static readonly Blake2BConfig DefaultConfig = new();

        private readonly Blake2BCore core;

        private readonly SecureArray<ulong> rawConfig;

        #nullable enable
        private readonly SecureArray<byte>? key;
        #nullable restore

        #nullable enable
        private readonly byte[]? defaultOutputBuffer;
        #nullable restore

        private readonly int outputSizeInBytes;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Blake2BHasher"/> class.
        /// </summary>
        /// <param name="config">The configuration to use; may be null to use the default Blake2 configuration.</param>
        /// <param name="secureArrayCall">Used to create <see cref="SecureArray"/> instances.</param>
        #nullable enable
        public Blake2BHasher(Blake2BConfig? config, SecureArrayCall secureArrayCall)
        #nullable restore
        {
            config ??= DefaultConfig;
            this.core = new Blake2BCore(secureArrayCall, config.LockMemoryPolicy);
            this.rawConfig = Blake2IvBuilder.ConfigB(config, null, secureArrayCall);
            if (config.Key != null && config.Key.Length != 0)
            {
                switch (config.LockMemoryPolicy)
                {
                    case LockMemoryPolicy.None:
                        this.key = new SecureArray<byte>(128, SecureArrayType.ZeroedAndPinned, secureArrayCall);
                        break;
                    case LockMemoryPolicy.BestEffort:
                        try
                        {
                            this.key = new SecureArray<byte>(128, SecureArrayType.ZeroedPinnedAndNoSwap, secureArrayCall);
                        }
                        catch (LockFailException)
                        {
                            this.key = new SecureArray<byte>(128, SecureArrayType.ZeroedAndPinned, secureArrayCall);
                        }

                        break;
                    default:
                        this.key = new SecureArray<byte>(128, SecureArrayType.ZeroedPinnedAndNoSwap, secureArrayCall);
                        break;
                }

                Array.Copy(config.Key, this.key.Buffer, config.Key.Length);
            }

            this.outputSizeInBytes = config.OutputSizeInBytes;
            this.defaultOutputBuffer = config.Result64ByteBuffer;
            this.Init();
        }

        /// <summary>
        /// Initialize the hasher. The hasher is initialized upon construction but this can be used
        /// to reinitialize in order to reuse the hasher.
        /// </summary>
        /// <exception cref="ObjectDisposedException">When called after being disposed.</exception>
        public sealed override void Init()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Called Blake2BHasher.Init() on disposed object");
            }

            this.core.Initialize(this.rawConfig.Buffer);
            if (this.key != null)
            {
                this.core.HashCore(this.key.Buffer, 0, this.key.Buffer.Length);
            }
        }

        /// <summary>
        /// Update the hasher with more bytes of data.
        /// </summary>
        /// <param name="data">Buffer holding the data to update with.</param>
        /// <param name="start">The offset into the buffer of the data to update the hasher with.</param>
        /// <param name="count">The number of bytes starting at <paramref name="start"/> to update the hasher with.</param>
        /// <exception cref="ObjectDisposedException">When called after being disposed.</exception>
        public override void Update(byte[] data, int start, int count)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Called Blake2BHasher.Update() on disposed object");
            }

            this.core.HashCore(data, start, count);
        }

        /// <summary>
        /// Either returns <see cref="Blake2BConfig"/>.<see cref="Blake2BConfig.Result64ByteBuffer"/>
        /// or a new buffer of <see cref="Blake2BConfig"/>.<see cref="Blake2BConfig.OutputSizeInBytes"/>
        /// if no <see cref="Blake2BConfig.Result64ByteBuffer"/> was given.
        /// </summary>
        /// <returns>
        /// Either the final Blake2 hash or the <see cref="Blake2BConfig.Result64ByteBuffer"/>. If
        /// <see cref="Blake2BConfig.Result64ByteBuffer"/> is non-null and <see cref="Blake2BConfig"/>.<see
        /// cref="Blake2BConfig.OutputSizeInBytes"/> is less than 64, then the actual Blake2 hash
        /// is the first <see cref="Blake2BConfig.OutputSizeInBytes"/> of the <see
        /// cref="Blake2BConfig.Result64ByteBuffer"/> buffer.
        /// </returns>
        public override byte[] Finish()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Called Blake2BHasher.Finish() on disposed object");
            }

            if (this.defaultOutputBuffer != null)
            {
                return this.core.HashFinal(this.defaultOutputBuffer);
            }

            byte[] fullResult = this.core.HashFinal();
            if (this.outputSizeInBytes != fullResult.Length)
            {
                var result = new byte[this.outputSizeInBytes];
                Array.Copy(fullResult, result, result.Length);
                return result;
            }

            return fullResult;
        }

        /// <summary>
        /// Disposes resources if <paramref name="disposing"/> is true.
        /// </summary>
        /// <param name="disposing">
        /// Set to true if disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.key?.Dispose();
            this.rawConfig.Dispose();
            this.core.Dispose();
            this.disposed = true;
            base.Dispose(disposing);
        }
    }
}
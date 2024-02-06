#region "Using"
using System;
using System.Runtime.CompilerServices;
#endregion

namespace SHA3M.Security.Cryptography
{
    public abstract class SHA3Managed : SHA3
    {
        private int Kecc_R;
        private int hashSize;

        public override int HashSize => hashSize;
        protected int SizeInBytes => Kecc_R / 8;
        protected int HashByteLength => HashSize / 8;

        private const int Kecc_KB               = 1600;
        private const int Kecc_NumRounds        = 24;
        private const int Kecc_LaneSizeBits     = 8 * 8;

        private static readonly ulong[] RoundConstants = new ulong[]
        {
            0x0000000000000001UL,
            0x0000000000008082UL,
            0x800000000000808aUL,
            0x8000000080008000UL,
            0x000000000000808bUL,
            0x0000000080000001UL,
            0x8000000080008081UL,
            0x8000000000008009UL,
            0x000000000000008aUL,
            0x0000000000000088UL,
            0x0000000080008009UL,
            0x000000008000000aUL,
            0x000000008000808bUL,
            0x800000000000008bUL,
            0x8000000000008089UL,
            0x8000000000008003UL,
            0x8000000000008002UL,
            0x8000000000000080UL,
            0x000000000000800aUL,
            0x800000008000000aUL,
            0x8000000080008081UL,
            0x8000000000008080UL,
            0x0000000080000001UL,
            0x8000000080008008UL
        };

        private ulong[] state;
        private byte[] buffer;
        private int len_Buffer;

        /*
            Values are listed on 
                https://en.wikipedia.org/wiki/SHA-3
        */

        internal SHA3Managed( int len_HashBit )
        {
            if ( len_HashBit != 128 && len_HashBit != 224 && len_HashBit != 256 && len_HashBit != 384 && len_HashBit != 512 )
                throw new ArgumentException( "len_HashBit requires 128, 224, 256, 384, or 512", nameof( len_HashBit ) );

            hashSize = len_HashBit;
            switch ( len_HashBit )
            {
                case 128:
                    Kecc_R = 1344;
                    break;

                case 224:
                    Kecc_R = 1152;
                    break;

                case 256:
                    Kecc_R = 1088;
                    break;

                case 384:
                    Kecc_R = 832;
                    break;

                case 512:
                    Kecc_R = 576;
                    break;
            }

            Initialize();
        }

        public override void Initialize( )
        {
            buffer      = new byte[ SizeInBytes ];
            len_Buffer  = 0;
            state       = new ulong[ 5 * 5 ]; // 1600 bits ( 5 * 5 * 64 )
        }

        private void AddToBuffer( ReadOnlySpan<byte> array, ref int offset, ref int count )
        {
            int amt = Math.Min( count, buffer.Length - len_Buffer );
            array.Slice( offset, amt ).CopyTo(new Span<byte>( buffer, len_Buffer, amt ) );

            offset      += amt;
            len_Buffer  += amt;
            count       -= amt;
        }

        protected override void HashCore( byte[] arr, int ibStart, int cbSize )
        {
            base.HashCore( arr, ibStart, cbSize );
            if ( cbSize == 0 ) return;

            int stride      = SizeInBytes >> 3;
            ulong[] utemps  = new ulong[ stride ];

            if ( len_Buffer == SizeInBytes )
                throw new Exception( "Unexpected error, buffer full" );

            AddToBuffer( arr, ref ibStart, ref cbSize );
            if (len_Buffer == SizeInBytes) //buffer full
            {
                Buffer.BlockCopy( buffer, 0, utemps, 0, SizeInBytes );
                KeccakF( utemps, stride );
                len_Buffer = 0;
            }

            for (; cbSize >= SizeInBytes; cbSize -= SizeInBytes, ibStart += SizeInBytes )
            {
                Buffer.BlockCopy( arr, ibStart, utemps, 0, SizeInBytes );
                KeccakF( utemps, stride );
            }

            if ( cbSize > 0 ) //remaining
            {
                Buffer.BlockCopy( arr, ibStart, buffer, len_Buffer, cbSize );
                len_Buffer += cbSize;
            }
        }

        protected override byte[] HashFinal( )
        {
            byte[] outb = new byte[ HashByteLength ];
            Array.Clear( buffer, len_Buffer, SizeInBytes - len_Buffer ); // padding

            if ( bUseKeccPadding )
                buffer[ len_Buffer++ ] = 1;
            else
                buffer[ len_Buffer++ ] = 6;

            buffer[ SizeInBytes - 1 ] |= 0x80;

            int stride      = SizeInBytes >> 3;
            ulong[] utemps  = new ulong[ stride ];

            Buffer.BlockCopy(buffer, 0, utemps, 0, SizeInBytes);

            KeccakF(utemps, stride);

            Buffer.BlockCopy(state, 0, outb, 0, HashByteLength);

            return outb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ulong ROL(ulong a, int offset) => ((a) << (offset % Kecc_LaneSizeBits)) ^ ((a) >> (Kecc_LaneSizeBits - (offset % Kecc_LaneSizeBits)));

        private void KeccakF( Span<ulong> inb, int laneCount )
        {
            Span<ulong> state               = this.state;
            ReadOnlySpan<ulong> RndConst    = SHA3Managed.RoundConstants;

            while ( --laneCount >= 0 )
                state[ laneCount ] ^= inb[ laneCount ];

            ulong Aba, Abe, Abi, Abo, Abu;
            ulong Aga, Age, Agi, Ago, Agu;
            ulong Aka, Ake, Aki, Ako, Aku;
            ulong Ama, Ame, Ami, Amo, Amu;
            ulong Asa, Ase, Asi, Aso, Asu;
            ulong BCa, BCe, BCi, BCo, BCu;
            ulong Da, De, Di, Do, Du;
            ulong Eba, Ebe, Ebi, Ebo, Ebu;
            ulong Ega, Ege, Egi, Ego, Egu;
            ulong Eka, Eke, Eki, Eko, Eku;
            ulong Ema, Eme, Emi, Emo, Emu;
            ulong Esa, Ese, Esi, Eso, Esu;

            Aba = state[ 0 ];
            Abe = state[ 1 ];
            Abi = state[ 2 ];
            Abo = state[ 3 ];
            Abu = state[ 4 ];
            Aga = state[ 5 ];
            Age = state[ 6 ];
            Agi = state[ 7 ];
            Ago = state[ 8 ];
            Agu = state[ 9 ];
            Aka = state[ 10 ];
            Ake = state[ 11 ];
            Aki = state[ 12 ];
            Ako = state[ 13 ];
            Aku = state[ 14 ];
            Ama = state[ 15 ];
            Ame = state[ 16 ];
            Ami = state[ 17 ];
            Amo = state[ 18 ];
            Amu = state[ 19 ];
            Asa = state[ 20 ];
            Ase = state[ 21 ];
            Asi = state[ 22 ];
            Aso = state[ 23 ];
            Asu = state[ 24 ];

            for (int round = 0; round < Kecc_NumRounds; round += 2)
            {
                // Theta
                BCa = Aba ^ Aga ^ Aka ^ Ama ^ Asa;
                BCe = Abe ^ Age ^ Ake ^ Ame ^ Ase;
                BCi = Abi ^ Agi ^ Aki ^ Ami ^ Asi;
                BCo = Abo ^ Ago ^ Ako ^ Amo ^ Aso;
                BCu = Abu ^ Agu ^ Aku ^ Amu ^ Asu;

                Da = BCu ^ ROL(BCe, 1);
                De = BCa ^ ROL(BCi, 1);
                Di = BCe ^ ROL(BCo, 1);
                Do = BCi ^ ROL(BCu, 1);
                Du = BCo ^ ROL(BCa, 1);

                Aba ^= Da;
                BCa = Aba;
                Age ^= De;
                BCe = ROL(Age, 44);
                Aki ^= Di;
                BCi = ROL(Aki, 43);
                Amo ^= Do;
                BCo = ROL(Amo, 21);
                Asu ^= Du;
                BCu = ROL(Asu, 14);
                Eba = BCa ^ ( ( ~BCe ) & BCi );
                Eba ^= RndConst[round];
                Ebe = BCe ^ ( ( ~BCi ) & BCo );
                Ebi = BCi ^ ( ( ~BCo ) & BCu );
                Ebo = BCo ^ ( ( ~BCu ) & BCa );
                Ebu = BCu ^ ( ( ~BCa ) & BCe );

                Abo ^= Do;
                BCa = ROL(Abo, 28);
                Agu ^= Du;
                BCe = ROL(Agu, 20);
                Aka ^= Da;
                BCi = ROL(Aka, 3);
                Ame ^= De;
                BCo = ROL(Ame, 45);
                Asi ^= Di;
                BCu = ROL(Asi, 61);
                Ega = BCa ^ ((~BCe) & BCi);
                Ege = BCe ^ ((~BCi) & BCo);
                Egi = BCi ^ ((~BCo) & BCu);
                Ego = BCo ^ ((~BCu) & BCa);
                Egu = BCu ^ ((~BCa) & BCe);

                Abe ^= De;
                BCa = ROL( Abe, 1 );
                Agi ^= Di;
                BCe = ROL( Agi, 6 );
                Ako ^= Do;
                BCi = ROL( Ako, 25 );
                Amu ^= Du;
                BCo = ROL( Amu, 8 );
                Asa ^= Da;
                BCu = ROL( Asa, 18 );
                Eka = BCa ^ ( ( ~BCe ) & BCi );
                Eke = BCe ^ ( ( ~BCi ) & BCo );
                Eki = BCi ^ ( ( ~BCo ) & BCu );
                Eko = BCo ^ ( ( ~BCu ) & BCa );
                Eku = BCu ^ ( ( ~BCa ) & BCe );

                Abu ^= Du;
                BCa = ROL(Abu, 27);
                Aga ^= Da;
                BCe = ROL(Aga, 36);
                Ake ^= De;
                BCi = ROL(Ake, 10);
                Ami ^= Di;
                BCo = ROL(Ami, 15);
                Aso ^= Do;
                BCu = ROL(Aso, 56);
                Ema = BCa ^ ((~BCe) & BCi);
                Eme = BCe ^ ((~BCi) & BCo);
                Emi = BCi ^ ((~BCo) & BCu);
                Emo = BCo ^ ((~BCu) & BCa);
                Emu = BCu ^ ((~BCa) & BCe);

                Abi ^= Di;
                BCa = ROL(Abi, 62);
                Ago ^= Do;
                BCe = ROL(Ago, 55);
                Aku ^= Du;
                BCi = ROL(Aku, 39);
                Ama ^= Da;
                BCo = ROL(Ama, 41);
                Ase ^= De;
                BCu = ROL(Ase, 2);
                Esa = BCa ^ ((~BCe) & BCi);
                Ese = BCe ^ ((~BCi) & BCo);
                Esi = BCi ^ ((~BCo) & BCu);
                Eso = BCo ^ ((~BCu) & BCa);
                Esu = BCu ^ ((~BCa) & BCe);

                //    prepareTheta
                BCa = Eba ^ Ega ^ Eka ^ Ema ^ Esa;
                BCe = Ebe ^ Ege ^ Eke ^ Eme ^ Ese;
                BCi = Ebi ^ Egi ^ Eki ^ Emi ^ Esi;
                BCo = Ebo ^ Ego ^ Eko ^ Emo ^ Eso;
                BCu = Ebu ^ Egu ^ Eku ^ Emu ^ Esu;

                // thetaRhoPiChiIotaPrepareTheta(round+1, E, A)
                Da = BCu ^ ROL(BCe, 1);
                De = BCa ^ ROL(BCi, 1);
                Di = BCe ^ ROL(BCo, 1);
                Do = BCi ^ ROL(BCu, 1);
                Du = BCo ^ ROL(BCa, 1);

                Eba ^= Da;
                BCa = Eba;
                Ege ^= De;
                BCe = ROL(Ege, 44);
                Eki ^= Di;
                BCi = ROL(Eki, 43);
                Emo ^= Do;
                BCo = ROL(Emo, 21);
                Esu ^= Du;
                BCu = ROL(Esu, 14);
                Aba = BCa ^ ((~BCe) & BCi);
                Aba ^= RndConst[round + 1];
                Abe = BCe ^ ((~BCi) & BCo);
                Abi = BCi ^ ((~BCo) & BCu);
                Abo = BCo ^ ((~BCu) & BCa);
                Abu = BCu ^ ((~BCa) & BCe);

                Ebo ^= Do;
                BCa = ROL(Ebo, 28);
                Egu ^= Du;
                BCe = ROL(Egu, 20);
                Eka ^= Da;
                BCi = ROL(Eka, 3);
                Eme ^= De;
                BCo = ROL(Eme, 45);
                Esi ^= Di;
                BCu = ROL(Esi, 61);
                Aga = BCa ^ ((~BCe) & BCi);
                Age = BCe ^ ((~BCi) & BCo);
                Agi = BCi ^ ((~BCo) & BCu);
                Ago = BCo ^ ((~BCu) & BCa);
                Agu = BCu ^ ((~BCa) & BCe);

                Ebe ^= De;
                BCa = ROL(Ebe, 1);
                Egi ^= Di;
                BCe = ROL(Egi, 6);
                Eko ^= Do;
                BCi = ROL(Eko, 25);
                Emu ^= Du;
                BCo = ROL(Emu, 8);
                Esa ^= Da;
                BCu = ROL(Esa, 18);
                Aka = BCa ^ ((~BCe) & BCi);
                Ake = BCe ^ ((~BCi) & BCo);
                Aki = BCi ^ ((~BCo) & BCu);
                Ako = BCo ^ ((~BCu) & BCa);
                Aku = BCu ^ ((~BCa) & BCe);

                Ebu ^= Du;
                BCa = ROL(Ebu, 27);
                Ega ^= Da;
                BCe = ROL(Ega, 36);
                Eke ^= De;
                BCi = ROL(Eke, 10);
                Emi ^= Di;
                BCo = ROL(Emi, 15);
                Eso ^= Do;
                BCu = ROL(Eso, 56);
                Ama = BCa ^ ((~BCe) & BCi);
                Ame = BCe ^ ((~BCi) & BCo);
                Ami = BCi ^ ((~BCo) & BCu);
                Amo = BCo ^ ((~BCu) & BCa);
                Amu = BCu ^ ((~BCa) & BCe);

                Ebi ^= Di;
                BCa = ROL(Ebi, 62);
                Ego ^= Do;
                BCe = ROL(Ego, 55);
                Eku ^= Du;
                BCi = ROL(Eku, 39);
                Ema ^= Da;
                BCo = ROL(Ema, 41);
                Ese ^= De;
                BCu = ROL(Ese, 2);
                Asa = BCa ^ ((~BCe) & BCi);
                Ase = BCe ^ ((~BCi) & BCo);
                Asi = BCi ^ ((~BCo) & BCu);
                Aso = BCo ^ ((~BCu) & BCa);
                Asu = BCu ^ ((~BCa) & BCe);
            }

            state[ 0 ]  = Aba;
            state[ 1 ]  = Abe;
            state[ 2 ]  = Abi;
            state[ 3 ]  = Abo;
            state[ 4 ]  = Abu;
            state[ 5 ]  = Aga;
            state[ 6 ]  = Age;
            state[ 7 ]  = Agi;
            state[ 8 ]  = Ago;
            state[ 9 ]  = Agu;
            state[ 10 ] = Aka;
            state[ 11 ] = Ake;
            state[ 12 ] = Aki;
            state[ 13 ] = Ako;
            state[ 14 ] = Aku;
            state[ 15 ] = Ama;
            state[ 16 ] = Ame;
            state[ 17 ] = Ami;
            state[ 18 ] = Amo;
            state[ 19 ] = Amu;
            state[ 20 ] = Asa;
            state[ 21 ] = Ase;
            state[ 22 ] = Asi;
            state[ 23 ] = Aso;
            state[ 24 ] = Asu;

        }
    }
}
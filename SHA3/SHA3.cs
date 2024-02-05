/*
	@Title		: Algorithms > SHA-3
	@Website	: https://github.com/Aetherinox/xsum-shahash-utility
	@Authors	: Aetherinox

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
*/

#region "Using"
using System;
using System.Security.Cryptography;
#endregion

namespace SHA3M.Security.Cryptography
{
    public abstract class SHA3 : HashAlgorithm
    {

        #region Statics

            public static new SHA3 Create( )
            {
                return Create( "SHA3-256" );
            }

            public bool bUseKeccPadding { get; set; }

            public static new SHA3 Create(string name )
            {
                switch ( name.ToLower( ).Replace( "-", string.Empty ) )
                {
                    case "sha3224":
                    case "sha3224managed":
                        return new SHA3224Managed( );

                    case "sha3":
                    case "sha3256":
                    case "sha3256managed":
                        return new SHA3256( );

                    case "sha3384":
                    case "sha3384managed":
                        return new SHA3384Managed( );

                    case "sha3512":
                    case "sha3512managed":
                        return new SHA3512Managed( );

                    default:
                        return null;
                }
            }

        #endregion

        protected override void HashCore( byte[] arr, int ibStart, int cbSize )
        {
            if ( arr == null )
                throw new ArgumentNullException( "array" );

            if ( ibStart < 0 )
                throw new ArgumentOutOfRangeException( "ibStart" );

            if ( cbSize > arr.Length )
                throw new ArgumentOutOfRangeException( "cbSize" );

            if ( ibStart + cbSize > arr.Length )
                throw new ArgumentOutOfRangeException( "ibStart or cbSize" );
        }
    }
}

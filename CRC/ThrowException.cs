/*
	@Title		: Algorithms > CRC
	@Website	: https://github.com/Aetherinox/xsum-shahash-utility
	@Authors	: Aetherinox
				: Steve Whitley

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
using System.Runtime.Serialization;

#endregion

namespace Aetherx.Algo
{

    [Serializable]
    public class ThrowException : Exception
    {
        public CRC16 Algo16 { get; set; }

        public CRC8 Algo8 { get; set; }

        public ThrowException ( ) { }

        public ThrowException ( CRC16 algo )
        {
            this.Algo16 = algo;
        }

        public ThrowException ( CRC8 algo )
        {
            this.Algo8 = algo;
        }

        public ThrowException ( string msg ) : base ( msg ) { }

        public ThrowException ( string msg, Exception innerException ) : base ( msg, innerException ) { }

        protected ThrowException ( SerializationInfo info, StreamingContext context ) : base ( info, context ) { }
    }
}
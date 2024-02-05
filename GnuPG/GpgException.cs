/*
	@Title		: Algorithms > CRC
	@Website	: https://github.com/Aetherinox/xsum-shahash-utility
	@Authors	: Aetherinox
                : Benton Stark

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

using System;
using System.Runtime.Serialization;

namespace Starksoft.Aspen.GnuPG
{

    /*
        This exception is thrown when a general, unexpected error condition occurs when running the GPG.EXE program.  
    */

    [ Serializable( ) ]
    public class GpgException : Exception
    {

        /*
            GPG Exception > Constructor
        */

        public GpgException( ) { }

        /*
            GPG Exception > Constructor

            :   msg
                Exception message text.
        */

        public GpgException( string msg ) : base( msg ) { }

        /*
            GPG Exception > Constructor

            :   message
                Exception message text.

            :   innerException
                The inner exception object.
        */

        public GpgException( string msg, Exception innerException ) : base( msg, innerException ) { }

        /*
            GPG Exception > Constructor

            :   info
                Serialization information.

            :   context
                Stream context information.
        */

        protected GpgException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }  
}

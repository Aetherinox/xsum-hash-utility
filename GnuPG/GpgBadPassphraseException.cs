/*
	@Title		: Algorithms > CRC
	@Website	: https://github.com/Aetherinox/XSum-shahash-utility
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
        This exception is thrown when a bad passphrase is given resulting in an error condition when running the GPG.EXE program.   
    */

    [ Serializable( ) ]
    public class GpgBadPassphraseException : Exception
    {

        /*
            GPG Bad Passphrase > Constructor
        */

        public GpgBadPassphraseException( ) { }

        /*
            GPG Bad Passphrase > Constructor

            :   message
                Exception message text.
        */

        public GpgBadPassphraseException( string message ) : base( message ) { }

        /*
            GPG Bad Passphrase > Constructor

            :   message
                Exception message text.

            :   innerException
                The inner exception object.
        */

        public GpgBadPassphraseException( string message, Exception innerException ) : base( message, innerException ) { }

        /*
            GPG Bad Passphrase > Constructor

            :   info
                Serialization information.

            :   context
                Stream context information
        */

        protected GpgBadPassphraseException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
    }  
}


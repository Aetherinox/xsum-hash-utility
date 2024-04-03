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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Starksoft.Aspen.GnuPG
{

    /*
        GPG Version Parser

        GPG command line version parser. This class parses the GPG stdout data in the expected format
            gpg GnuPG major.minor.revision

        Where major and minor are expected to be integers and revsision can be an integer or string.

        Examples:
            gpg (GnuPG) 2.5.1
            gpg (GnuPG) 2.5.1-beta1
    */

    class GpgVersionParser
    {

        /*
            Regex filtering for version
        */

        private static Regex _gpgVersion = new Regex( @"(?<=gpg \(GnuPG\) )([0-9]*\.[0-9]*\..*)", RegexOptions.Compiled );

        /*
            Private Constructor
        */

        private GpgVersionParser() {  }

        /*
            Parse the GPG version information from the Stdout of the GPG command line interface

            :   data
                stdout stream data

            :   return
                GPG version object
        */

        public static GpgVersion Parse( StreamReader data )
        {
            string firstLine    = ReadFirstStdOutLine( data );
            GpgVersion ver      = ParseVersion( firstLine );

            return ver;
        }

        /*
            Read First Line
        */

        private static string ReadFirstStdOutLine( StreamReader data )
        {
            return data.ReadLine( );
        }

        /*
            GPG Version
        */

        private static GpgVersion ParseVersion( string line )
        {

            if ( !_gpgVersion.IsMatch( line ) )
                throw new GpgException(String.Format( "unexpected gpg command line version data '{0}'", line ) );

            string[] version    = _gpgVersion.Match( line ).ToString( ).Split( '.' );
            int major           = Int32.Parse(version[ 0 ]);
            int minor           = Int32.Parse(version[ 1 ]);
            string revision     = version[ 2 ];

            return new GpgVersion(major, minor, revision);
        }

    }


}

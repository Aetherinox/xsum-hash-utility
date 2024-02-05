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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starksoft.Aspen.GnuPG
{

    /*
        GPG version number class.
    */

    public class GpgVersion
    {
        private int _major;
        private int _minor;
        private string _revision;

        /*
            Constructor
        */

        public GpgVersion( int major, int minor, string revision )
        {
            _major      = major;
            _minor      = minor;
            _revision   = revision;
        }

        /*
            Gets the GPG major version number.

            :   return
                Major version number
        */

        public int Major
        {
            get { return _major; }
        }

        /*
            Gets the GPG minor version number.

            :   return
                Minor version number
        */

        public int Minor
        {
            get { return _minor; }
        }

        /*
            Gets the GPG revision string.

            :   return
                Revision string
        */

        public string Revision
        {
            get { return _revision; }
        }
    }
}

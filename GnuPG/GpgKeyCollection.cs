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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;

namespace Starksoft.Aspen.GnuPG
{

    /*
        Collection of PGP keys stored in the GnuPGP application.
    */

    public class GpgKeyCollection : IEnumerable<GpgKey>
    {
        private List<GpgKey> _keyList = new List<GpgKey>( );
        private string _raw;

        private static string COL_KEY               = "Key";
        private static string COL_KEY_EXPIRATION    = "KeyExpiration";
        private static string COL_USER_ID           = "UserId";
        private static string COL_USER_NAME         = "UserName";
        private static string COL_SUB_KEY           = "SubKey";
        private static string COL_SUB_KEY_EXPIRATION = "SubKeyExpiration";
        private static string COL_RAW               = "Raw";
        
        /*
            Constructor

            :   keys
                StreamReader object containing GnuPG raw key stream data.
        */

        public GpgKeyCollection( StreamReader keys )
        {
            Fill( keys );
            GetRaw( keys );
        }

        /*
            Raw key stream text data.
        */

        public string Raw
        {
            get { return _raw; }
        }

        /*
            Get Raw
        */

        private void GetRaw( StreamReader keys )
        {
            keys.BaseStream.Position = 0;
            _raw = keys.ReadToEnd( );
        }

        /*
            Fill
        */

        private void Fill( StreamReader data )
        {
			StringBuilder sb = new StringBuilder ( );
                        
            while ( !data.EndOfStream )
            {
                // read a line from the output stream
                string line = data.ReadLine( );

                // skip lines we are not interested in
                if ( !line.StartsWith( "pub" ) && !line.StartsWith( "sec" ) && !line.StartsWith( "uid" ) )
                {
                    // make sure this isn't the end of a key parsing operation
                    if ( sb.Length != 0 )
                    {
						_keyList.Add(new GpgKey( sb.ToString( ) ) );
						sb = new StringBuilder ( );  // clear out the string builder
                    }

                    continue;
                }

				sb.AppendLine( line );
            }
        }

        /*
            Searches for the specified GnuPGKey object and returns the zero-based index of the
            first occurrence within the entire GnuPGKeyCollection colleciton.

            :   item
                The GnuPGKeyobject to locate in the GnuPGKeyCollection

            :   return
                The zero-based index of the first occurrence of item within the entire GnuPGKeyCollection, if found; otherwise, –1.
        */

        public int IndexOf( GpgKey item )
        {
            return _keyList.IndexOf( item );
        }

        /*
            Retrieves the specified GnuPGKey object by zero-based index from the GnuPGKeyCollection.

            :   index
                Zero-based index integer value

            :   return
                The GnuPGKey object corresponding to the index position
        */

        public GpgKey GetKey( int index )
        { 
            return _keyList[ index ]; 
        }

        /*
            Adds a GnuPGKey object to the end of the GnuPGKeyCollection.

            :   item
                GnuPGKey item to add to the GnuPGKeyCollection.
        */

        public void AddKey( GpgKey item )
        {
            _keyList.Add( item );
        }

        /*
            Gets the number of elements actually contained in the GnuPGKeyCollection.
        */

        public int Count
        { 
            get { return _keyList.Count; } 
        }

        /*
            GPG Key
        */

        IEnumerator<GpgKey> IEnumerable<GpgKey>.GetEnumerator( )
        { 
            return _keyList.GetEnumerator( ); 
        }

        /*
            Get Enumerator
        */

        IEnumerator IEnumerable.GetEnumerator( )
        {
            return _keyList.GetEnumerator( );
        }

        /*
            Indexer for the GnuPGKeyCollection collection.

            :   index
                Zero-based index value.
        */

        public GpgKey this[ int index ]
        {
            get
            {
                return _keyList[ index ];
            }

        }

        /*
            Convert current GnuPGKeyCollection to a DataTable object to make data binding a minpulation of key data easier.

            :   return
                Data table object.
        */

        public DataTable ToDataTable( )
        {
            DataTable dataTbl = new DataTable( );
            CreateColumns( dataTbl );

            foreach ( GpgKey item in _keyList )
            {
                DataRow row                     = dataTbl.NewRow( );
                row[ COL_USER_ID ]              = item.UserId;
                row[ COL_USER_NAME ]            = item.UserName;
                row[ COL_KEY ]                  = item.Key;
                row[ COL_KEY_EXPIRATION ]       = item.KeyExpiration;
                row[ COL_SUB_KEY ]              = item.SubKey;
                row[ COL_SUB_KEY_EXPIRATION ]   = item.SubKeyExpiration;

                dataTbl.Rows.Add( row );
            }

            return dataTbl;
        }

        /*
            Create Columns
        */

        private void CreateColumns( DataTable dataTbl )
        {
            dataTbl.Columns.Add( new DataColumn( COL_USER_ID,               typeof( string ) ) );
            dataTbl.Columns.Add( new DataColumn( COL_USER_NAME,             typeof( string ) ) );
            dataTbl.Columns.Add( new DataColumn( COL_KEY,                   typeof( string ) ) );
            dataTbl.Columns.Add( new DataColumn( COL_KEY_EXPIRATION,        typeof( DateTime ) ) );
            dataTbl.Columns.Add( new DataColumn( COL_SUB_KEY,               typeof( string ) ) );
            dataTbl.Columns.Add( new DataColumn( COL_SUB_KEY_EXPIRATION,    typeof( DateTime ) ) );
            dataTbl.Columns.Add( new DataColumn( COL_RAW,                   typeof( string) ) );
        }

    }
}

/*
    @app        : MobaXterm Keygen
    @repo       : https://github.com/Aetherinox/MobaXtermKeygen
    @author     : Aetherinox
*/

#region "Using"

using XSum;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SHA3M.Security.Cryptography;

#endregion

namespace XSum
{
    public sealed class Hash
    {

        #region "Define: Fileinfo"

            /*
                Define > File Name
                    utilized with logging
            */

            readonly static string log_file = "Hash.cs";

        #endregion

        /*
            Define > Classes
        */

        static AppInfo AppInfo             = new AppInfo( );

        /*
            method : hash
        */

        private Hash( ) {}

        /*
           cryptographic service provider
        */

        private static MD5 Md5              = MD5.Create( );
        private static SHA256 Sha256        = SHA256.Create( );

        /*
            method : string to byte array

            @ret    : byteArr
        */

        private static byte[] ConvertStringToByteArray( string data )
        {
            return ( new System.Text.UnicodeEncoding( ) ).GetBytes( data );
        }

        /*
            method : bytes to string

            converted string will be transformed to UPPERCASE;
            user should use --lowercase to match non-uppercase hashes

            @ret    : str
        */

        public static string BytesToString( byte[] bytes )
        {
            string result = "";
            foreach ( byte b in bytes ) result += b.ToString( "x2" ).ToUpper( );
            return result;
        }

        /*
            method : file stream

            @arg    : str path
        */

        private static System.IO.FileStream GetFileStream( string path )
        {
            return ( new System.IO.FileStream( path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite ) );
        }

        /*
            Get Hash
        */

        private static string GetHash( HashAlgorithm algorithm, string input )
        {

            byte[] data     = algorithm.ComputeHash( Encoding.UTF8.GetBytes( input ) );
            var sb          = new StringBuilder( );

            for ( int i = 0; i < data.Length; i++ )
            {
                sb.Append( data[ i ].ToString( "x2" ) );
            }

            return sb.ToString( );
        }

        /*
            Verify hash against string
        */

        private static bool VerifyHash( HashAlgorithm algorithm, string input, string hash )
        {
            var hashOfInput             = GetHash( algorithm, input );
            StringComparer comparer     = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare( hashOfInput, hash ) == 0;
        }


        /*
            Double Hashing Functionality

            @usage      : byte[] text = Encoding.UTF8.GetBytes( "Text );
                          var asd = Hash.Compute( text, 2 );
                          var asd = Hash.Compute( "sha256", text, 2 );
        */

        #region "Cryptography: Double Hash"

            public static byte[] Compute( string algorithm, byte[] input, int offset, int count )
            {
                using ( var hash = ( HashAlgorithm)CryptoConfig.CreateFromName( algorithm ) )
                {
                    return hash.ComputeHash( input, offset, count );
                }
            }

            public static byte[] Compute( string algorithm, byte[] input ) =>
                Compute( algorithm, input, 0, input.Length );

            public static byte[] Compute( string algorithm, byte[] input, int offset, int count, int iters )
            {
                if ( iters <= 0 )
                    throw new ArgumentException( "Iterations must be greater than zero", nameof( iters ) );

                var result = Compute( algorithm, input, offset, count );

                for ( var i = 0; i < iters - 1; ++i )
                    result = Compute( algorithm, result );

                return result;
            }

            public static byte[] Compute( string algorithm, byte[] input, int iters ) =>
                Compute( algorithm, input, 0, input.Length, iters );

        #endregion



        /*
            Method > Hash > Strings
            returns hash for string

            @arg    : str algo
            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: Hash > Strings"

        /*
            MD5, SHA-2
        */

        public static string GetHash_String_SHA2( string algo, string str )
            {
                using (var hash = (HashAlgorithm)CryptoConfig.CreateFromName( algo ) )
                {
                    byte[] bytes    = hash.ComputeHash( Encoding.UTF8.GetBytes( str ) );
                    return BytesToString( bytes );
                }
            }

            /*
                SHA-3
            */

            public static string GetHash_String_SHA3( string algo, string str )
            {
                using (var hash = SHA3.Create( algo ) )
                {
                    byte[] bytes    = hash.ComputeHash( Encoding.UTF8.GetBytes( str ) );
                    return BytesToString( bytes );
                }
            }

        #endregion



        /*
            Method > Hash > File
            returns hash for specified file

            @arg    : str algo
            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: Hash > File"

            /*
                MD5, SHA-2
            */

            public static string GetHash_File_SHA2( string algo, string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {
                    using ( var hash = (HashAlgorithm)CryptoConfig.CreateFromName( algo ) )
                    {
                        byte[] bytes    = hash.ComputeHash( stream );
                        return BytesToString( bytes );
                    }
                }
            }

            /*
                SHA-3
            */

            public static string GetHash_File_SHA3( string algo, string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {
                    SHA3 sha3       = SHA3.Create( algo );
                    byte[] bytes    = sha3.ComputeHash( stream );

                    return BytesToString( bytes );
                }
            }


        #endregion



        /*
            Method > Hash > Directory
            returns hash for specified directory

            @arg    : str algo
            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: Hash > Directory"

            /*
                MD5, SHA-2
            */

            static public string GetHash_Directory_SHA2( string algorithm, string folder )
            {

                var files                   = Directory.GetFiles( folder, "*.*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );
                using (var file_total       = (HashAlgorithm)CryptoConfig.CreateFromName( algorithm ) )
                {
                    int bytes_read_chunk    = 2048;

                    foreach ( string file in files )
                    {
                        using (var file_single = (HashAlgorithm)CryptoConfig.CreateFromName( algorithm ) )
                        {
                            using ( FileStream file_contents = File.OpenRead( file ) )
                            {
                                byte[] bytes_content    = new byte[ bytes_read_chunk ];
                                int bytes_read          = 0;

                                // (optimization) read file in chunks
                                while ( ( bytes_read = file_contents.Read( bytes_content, 0, bytes_read_chunk ) ) > 0 )
                                {
                                    file_total.TransformBlock   ( bytes_content, 0, bytes_read, bytes_content, 0 );
                                    file_single.TransformBlock  ( bytes_content, 0, bytes_read, bytes_content, 0 );
                                }

                                // close file_single block with 0 length
                                file_single.TransformFinalBlock( bytes_content, 0, 0 );
                            }

                            if ( AppInfo.bIsDebug( ) )
                            {
                                Console.WriteLine( file );
                                Console.WriteLine( BitConverter.ToString( file_single.Hash ).Replace( "-", "" ).ToUpper( ) );
                                Console.WriteLine( "\n");
                            }

                        }
                    }

                    // close total hash block
                    file_total.TransformFinalBlock( new byte[ 0 ], 0, 0 );

                    return BytesToString( file_total.Hash );
                }
            }

            /*
                SHA-3
            */

            static public string GetHash_Directory_SHA3( string algo, string folder )
            {

                var files                   = Directory.GetFiles( folder, "*.*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );
                using (var file_total       = SHA3.Create( algo ) )
                {
                    int bytes_read_chunk    = 2048;

                    foreach ( string file in files )
                    {
                        using (var file_single = SHA3.Create( algo ) )
                        {
                            using ( FileStream file_contents = File.OpenRead( file ) )
                            {
                                byte[] bytes_content    = new byte[ bytes_read_chunk ];
                                int bytes_read          = 0;

                                // (optimization) read file in chunks
                                while ( ( bytes_read = file_contents.Read( bytes_content, 0, bytes_read_chunk ) ) > 0 )
                                {
                                    file_total.TransformBlock   ( bytes_content, 0, bytes_read, bytes_content, 0 );
                                    file_single.TransformBlock  ( bytes_content, 0, bytes_read, bytes_content, 0 );
                                }

                                // close file_single block with 0 length
                                file_single.TransformFinalBlock( bytes_content, 0, 0 );
                            }

                            if ( AppInfo.bIsDebug( ) )
                            {
                                Console.WriteLine( file );
                                Console.WriteLine( BitConverter.ToString( file_single.Hash ).Replace( "-", "" ).ToUpper( ) );
                                Console.WriteLine( "\n");
                            }

                        }
                    }

                    // close total hash block
                    file_total.TransformFinalBlock( new byte[ 0 ], 0, 0 );

                    return BytesToString( file_total.Hash );
                }
            }



        #endregion



        /*
            Method > Management

            the initial methods called when getting hash for item
        */

        #region "Cryptography: Hash > Manage"

            /*
                Management > MD5, SHA1, SHA-2
            */

            public static string Hash_Manage_SHA2( string algo, string src )
            {
                if ( Directory.Exists( src ) )
                    return Hash.GetHash_Directory_SHA2( algo,  src );
                else if ( File.Exists( src ) )
                    return Hash.GetHash_File_SHA2( algo, src );
                else
                    return Hash.GetHash_String_SHA2( algo, src );
            }

            /*
                Management > SHA-3
            */

            public static string Hash_Manage_SHA3( string algo, string src )
            {
                if ( Directory.Exists( src ) )
                    return Hash.GetHash_Directory_SHA3( algo,  src );
                else if ( File.Exists( src ) )
                    return Hash.GetHash_File_SHA3( algo, src );
                else
                    return Hash.GetHash_String_SHA3( algo, src );
            }

        #endregion

    }

}

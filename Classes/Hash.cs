/*
    @app        : MobaXterm Keygen
    @repo       : https://github.com/Aetherinox/MobaXtermKeygen
    @author     : Aetherinox
*/

#region "Using"

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XSum;

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
            method : Get SHA1 Hash
                requires a path to file to obtain the SHA1 hash

            @arg    : str path
            @ret    : str
        */

        public static string ReturnSHA1( string path )
        {
            string result       = "";
            string res_hash     = "";

            byte[] arrByteHash;
            System.IO.FileStream fs = null;

            System.Security.Cryptography.SHA1CryptoServiceProvider sha1_hash = new System.Security.Cryptography.SHA1CryptoServiceProvider( );

            try
            {
                fs              = GetFileStream( path );
                arrByteHash     = sha1_hash.ComputeHash( fs );

                fs.Close( );

                res_hash        = System.BitConverter.ToString( arrByteHash );
                res_hash        = res_hash.Replace( "-", "" );
                result          = res_hash;
            }
            catch ( System.Exception ex )
            {
                Console.Write( String.Format( "Error {0}", ex.Message ) );
            }

            if ( AppInfo.bIsDebug( ) )
            {
                Console.Write( String.Format( "Result (SHA1): {0}", result ) );
            }


            return ( result );
        }

        /*
            method : Get MD5 Hash
                requires a path to file to obtain the md5 hash

            @arg    : str path
            @ret    : str
        */

        public static string ReturnMD5( string path )
        {
            string result       = "";
            string res_hash     = "";

            byte[] arrByteHash;
            System.IO.FileStream fs = null;

            System.Security.Cryptography.MD5CryptoServiceProvider sha1_hash = new System.Security.Cryptography.MD5CryptoServiceProvider( );

            try
            {
                fs              = GetFileStream( path );
                arrByteHash     = sha1_hash.ComputeHash( fs );

                fs.Close( );

                res_hash        = System.BitConverter.ToString( arrByteHash );
                res_hash        = res_hash.Replace( "-", "" );
                result          = res_hash;
            }
            catch ( System.Exception ex )
            {
                Console.Write( String.Format( "Error: {0}", ex.Message ) );
            }

            if ( AppInfo.bIsDebug( ) )
            {
                Console.Write( String.Format( "Result (MD5): {0}", result ) );
            }

            return ( result );
        }


        /*
            method : Get SHA256 Hash (string)
                requires a path to file to obtain the sha256 hash

            @arg    : str path
            @ret    : str
        */

        public static string GetHash_String( string algorithm, string str )
        {
            using (var hash = (HashAlgorithm)CryptoConfig.CreateFromName( algorithm ) )
            {
                byte[] bytes    = hash.ComputeHash( Encoding.UTF8.GetBytes( str ) );
                return BytesToString( bytes );
            }
        }

        /*
            Cryptography > MD5
                Generates an MD5 hash from the file stream path provided.

            @usage  : string MD5 = Hash.BytesToString( Hash.GetHash_MD5( "X:\Path\To\File.dll" ) );
                      string MD5 = Hash.GetHash_MD5( "X:\Path\To\File.dll" );

            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: MD5"

            public static string GetHash_MD5( string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {

                    byte[] bytes = Md5.ComputeHash( stream );
                    return BytesToString( bytes );
                }
            }

        #endregion

        /*
            Cryptography > SHA1
                Generates an SHA1 hash from the file stream path provided.

            DoD PKI policy prohibits the use of SHA1 as of December 2016

            @usage  : string SHA1 = Hash.BytesToString( Hash.GetHash_SHA1( "X:\Path\To\File.dll" ) );
                      string SHA1 = Hash.GetHash_SHA1( "X:\Path\To\File.dll" );

            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: SHA1"

            public static string GetHash_SHA1( string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {
                    SHA1 SHA1       = SHA1.Create( );
                    byte[] bytes    = SHA1.ComputeHash( stream );

                    return BytesToString( bytes );
                }
            }

        #endregion



        /*
            Cryptography > SHA2-256
                Generates an SHA256 hash from the file stream path provided.

            SHA1    : FIPS 180-1
            SHA2    : FIPS 180
            SHA3    : FIPS 202

            @usage  : string SHA256 = Hash.BytesToString( Hash.GetHash_SHA2_256( "X:\Path\To\File.dll" ) );
                      string SHA256 = Hash.GetHash_SHA2_256( "X:\Path\To\File.dll" );

            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: SHA2-256"

            public static string GetHash_SHA2_256( string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {
                    SHA256 sha256   = SHA256.Create( );
                    byte[] bytes    = Sha256.ComputeHash( stream );

                    return BytesToString( bytes );
                }
            }

        #endregion

        /*
            Cryptography > SHA2-384
                Generates an SHA384 hash from the file stream path provided.

            SHA2    : FIPS 180
            SHA3    : FIPS 202

            @usage  : string SHA384 = Hash.BytesToString( Hash.GetHash_SHA2_384( "X:\Path\To\File.dll" ) );
                      string SHA384 = Hash.GetHash_SHA2_384( "X:\Path\To\File.dll" );

            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: SHA2-384"

            public static string GetHash_SHA2_384( string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {
                    SHA384 Sha384   = SHA384.Create( );
                    byte[] bytes    = Sha384.ComputeHash( stream );

                    return BytesToString( bytes );
                }
            }

        #endregion



        /*
            Cryptography > SHA2-512
                Generates an SHA512 hash from the file stream path provided.

            SHA2    : FIPS 180
            SHA3    : FIPS 202

            @usage  : string SHA512 = Hash.BytesToString( Hash.GetHash_SHA2_512( "X:\Path\To\File.dll" ) );
                      string SHA512 = Hash.GetHash_SHA2_512( "X:\Path\To\File.dll" );

            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: SHA2-512"

            public static string GetHash_SHA2_512( string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {

                    SHA512 sha512   = SHA512.Create( );
                    byte[] bytes    = sha512.ComputeHash( stream );

                    return BytesToString( bytes );
                }
            }

        #endregion



        /*
            Cryptography > Get Hash
                Universal method for getting the hash of a file using the standards.
                md5, sha2.

            Generates a hash from the file stream path provided.

            SHA2    : FIPS 180
            SHA3    : FIPS 202

            @usage  : string hash = Hash.GetHash_Universe( "sha256", "x:\path\to\file.xxxx" );

            @arg    : str path
            @ret    : str
        */

        #region "Cryptography: Hash - File (Universal)"

            public static string GetHash_Universe( string algorithm, string path )
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {
                    using ( var hash = (HashAlgorithm)CryptoConfig.CreateFromName( algorithm ) )
                    {
                        byte[] bytes    = hash.ComputeHash( stream );
                        return BytesToString( bytes );
                    }
                }
            }

        #endregion



        public static string CreateMd5ForFolder( string path )
        {
            // assuming you want to include nested folders
            var files = Directory.GetFiles( path, "*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );

            MD5 md5 = MD5.Create( );

            for( int i = 0; i < files.Count; i++ )
            {
                string file             = files[ i ];
                string path_relative    = file.Substring( path.Length + 1 );
                byte[] path_bytes       = Encoding.UTF8.GetBytes( path_relative.ToLower( ) );

                md5.TransformBlock( path_bytes, 0, path_bytes.Length, path_bytes, 0 );
        
                // hash contents
                byte[] bytes_content = File.ReadAllBytes( file );
                if (i == files.Count - 1)
                    md5.TransformFinalBlock( bytes_content, 0, bytes_content.Length );
                else
                    md5.TransformBlock( bytes_content, 0, bytes_content.Length, bytes_content, 0 );
            }
    
            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }

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

        public static string CreateDirectoryMd5( string srcPath )
        {
            var filePaths = Directory.GetFiles( srcPath, "*", SearchOption.AllDirectories).OrderBy(p => p).ToArray();

            using (var sha256 = SHA256.Create())
            {
                foreach (var filePath in filePaths)
                {

                    // hash path
                    byte[] pathBytes = Encoding.UTF8.GetBytes(filePath);

                    Console.WriteLine( BytesToString( pathBytes ) );

                    sha256.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                    // hash contents
                    byte[] contentBytes = File.ReadAllBytes(filePath);

                    sha256.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
                }

                //Handles empty filePaths case
                sha256.TransformFinalBlock(new byte[0], 0, 0);

                return BitConverter.ToString(sha256.Hash).Replace("-", "").ToLower();
            }
        }



        // Verify a hash against a string.
        private static bool VerifyHash( HashAlgorithm algorithm, string input, string hash )
        {
            var hashOfInput             = GetHash( algorithm, input );
            StringComparer comparer     = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }

        /*
            Get SHA256 for files in folder
            Confirmed against sha256sum
        */

        static public string SHA256Hash( string folder )
        {

            if ( Directory.Exists( folder ) )
            {

                var files    = Directory.GetFiles( folder, "*", SearchOption.AllDirectories).OrderBy( p => p ).ToArray();
                var s       = new byte[ 64 ];
                byte[] m;
                byte[] x;

                foreach ( var file in files )
                {
                    byte[] path_bytes = Encoding.UTF8.GetBytes( file );

                    Console.WriteLine( BytesToString( path_bytes ) );
                    try
                    {
                        using ( IncrementalHash sha256 = IncrementalHash.CreateHash( HashAlgorithmName.SHA256 ) )
                        {
                            x = sha256.GetHashAndReset();
                        }

                        return BytesToString( x );
                    }
                    catch
                    {
                        Console.WriteLine( "Error" );
                    }
                   
                }

            }
            else
            {
                //Console.WriteLine("The directory specified could not be found.");
            }

            return string.Empty;
        }

        /*
            Allows you to return the hash for each individual file, as well as the
            overall folder hash.

            developed to be compatible with other hashing utilities.

            sha256sum for windows is broke, and returns the 0 byte hash for any folder.

            @arg    : str algorithm
            @arg    : str folder
        */

        #region "Cryptography: Hash - Directory (Universal)"

            static public string GetHash_Directory( string algorithm, string folder )
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

        #endregion


        /*
            Allows you to return the hash for each individual file, as well as the
            overall folder hash.

            developed to be compatible with other hashing utilities.

            sha256sum for windows is broke, and returns the 0 byte hash for any folder.

            @arg    : str algorithm
            @arg    : str folder
        */

        #region "Cryptography: Hash - Directory (Universal)"

            static public string GetHash_MD5_Directory( string directory )
            {

                var files               = Directory.GetFiles( directory, "*.*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );
                MD5 file_total          = MD5.Create( );
                int bytesToReadAtOnce   = 2048;
                var buffer              = new byte[ 8192 ];

                foreach ( string file in files )
                {
                    MD5 file_single = MD5.Create( );

                    // hash contents
                    // NOTE: Good for small files, high memory usage for large files
                    // may cause issues for other algorithms
                    // byte[] file_bytes = File.ReadAllBytes( file );
                    // file_single.TransformFinalBlock( file_bytes, 0, file_bytes.Length ); does not work with md5

                    using ( FileStream inputFile = File.OpenRead( file ) )
                    {
                        byte[] content  = new byte[ bytesToReadAtOnce ];
                        int bytesRead   = 0;

                        // Read the file only in chunks, allowing minimal memory usage.
                        while ( ( bytesRead = inputFile.Read( content, 0, bytesToReadAtOnce ) ) > 0 )
                        {
                            file_total.TransformBlock   ( content, 0, bytesRead, content, 0 );
                            file_single.TransformBlock  ( content, 0, bytesRead, content, 0 );
                        }

                        // Close file_single block with 0 length
                        file_single.TransformFinalBlock( content, 0, 0 );

                        Console.WriteLine( file );
                        Console.WriteLine( BitConverter.ToString( file_single.Hash ).Replace( "-", "" ).ToUpper( ) );
                        Console.WriteLine( "\n");
                    }
                }

                // close hash block
                file_total.TransformFinalBlock(new byte[ 0 ], 0, 0 );

                return BytesToString( file_total.Hash );
            }

        #endregion

        /*
            Double Hashing Functionality

            @usage      : byte[] text = Encoding.UTF8.GetBytes( "Text );
                          var asd = Hash.Compute( text, 2 );
                          var asd = Hash.Compute( "sha256", text, 2 );
        */

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

    }

}

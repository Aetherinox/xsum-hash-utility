﻿#region "Using"

using XSum;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SHA3M.Security.Cryptography;
using Blake2Fast;
using Aetherx.Algo;
using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using System.Threading;
using Cfg = XSum.Properties.Settings;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
        static Helpers Helpers             = new Helpers( );

        /*
            method : hash
        */

        private Hash( ) {}

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

        public static string BytesToString( byte[] a ) => string.Concat( a.Select( x => x.ToString( "X2" ).ToUpper( ) ) );

        /*
            method : file stream

            @arg    : str path
            @ret    : str
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
            Algo
        */

            /*
                CRC
            */

            public static string GetHash_String_CRC( string arg, string str )
            {
                int size   = 32;
                Int32.TryParse( arg, out size );

                var str_bytes       = System.Text.Encoding.UTF8.GetBytes ( str );

                if ( size == 8 )
                    return String.Format( "{0:X3}", Crc8.ComputeHash ( CRC8.Standard, str_bytes ) );
                else if ( size == 16 )
                    return String.Format( "{0:X4}", Crc16.ComputeHash ( CRC16.Standard, str_bytes ) );
                else 
                    return String.Format( "{0:X8}", Crc32.ComputeHash ( str_bytes ) );
            }

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

            /*
                Blake > 2B

                Blake 2 supports specified bytes

                    16      : 128
                    20      : 160
                    32      : 256
                    48      : 384
                    64      : 512
            */


            public static string GetHash_String_B2B( string arg, string str )
            {
                int hash_size   = 64;
                Int32.TryParse( arg, out hash_size );

                byte[] bytes    = Encoding.UTF8.GetBytes( str );
                var hash        = Blake2Fast.Blake2b.ComputeHash( hash_size, bytes );

                return BytesToString( hash );
            }

            /*
                Blake > 2S

                Blake 2 supports specified bytes

                    16      : 128
                    20      : 160
                    32      : 256
                    48      : 384
                    64      : 512
            */


            public static string GetHash_String_B2S( string arg, string str )
            {
                int hash_size   = 64;
                Int32.TryParse( arg, out hash_size );

                byte[] bytes    = Encoding.UTF8.GetBytes( str );
                var hash        = Blake2Fast.Blake2s.ComputeHash( hash_size, bytes );

                return BytesToString( hash );
            }

            /*
                Argon2

                Memory arg is initially a string since the user can input their own. Then convert to integer just because its easier.
            */

            public static string GetHash_String_AG2( string input, string salt, int memory = 1024, int len = 4, int iterations = 1, int threads = 1, int lanes = 1 )
            {

                byte[] input_bytes  = Encoding.UTF8.GetBytes( input );

                var config = new Argon2Config
                {
                    Type            = Argon2Type.DataIndependentAddressing,
                    Version         = Argon2Version.Nineteen,
                    TimeCost        = iterations,
                    MemoryCost      = memory,
                    Lanes           = lanes,
                    Threads         = threads, // higher than "Lanes" doesn't help (or hurt)
                    Password        = input_bytes,
                    Salt            = Encoding.UTF8.GetBytes( salt ), // >= 8 bytes if not null
                    HashLength      = len // needs > 4
                };

                var argon2A         = new Argon2(config);
                string hashString;

                using( SecureArray<byte> hashA = argon2A.Hash( ) )
                {
                    hashString = config.EncodeString(hashA.Buffer);
                }

                return hashString;
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
                Algo
            */

            public static string GetHash_File_CRC( string arg, string path )
            {
                int size   = 32;
                Int32.TryParse( arg, out size );

                using ( FileStream stream = File.OpenRead( path ) )
                {
                    if ( size == 8 )
                        return String.Format( "{0:X3}", Crc8.ComputeHash ( CRC8.Standard, Helpers.FSReadFull( stream ) ) );
                    else if ( size == 16 )
                        return String.Format( "{0:X4}", Crc16.ComputeHash ( CRC16.Standard, Helpers.FSReadFull( stream ) ) );
                    else 
                        return String.Format( "{0:X8}", Crc32.ComputeHash ( Helpers.FSReadFull( stream ) ) );
                }
            }

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

            /*
                Blake > 2B

                @arg        : str arg
                              number of bytes for the key

                @arg        : str path
                              path to file
            */

            public static string GetHash_File_B2B( string arg, string path  )
            {
                int hash_size = 64;
                Int32.TryParse( arg, out hash_size );

                using ( FileStream stream = File.OpenRead( path ) )
                {
                    byte[] bytes    = Blake2Fast.Blake2b.ComputeHash( hash_size, Helpers.FSReadFull( stream ) );
                    return BytesToString( bytes );
                }
            }

            /*
                Blake > 2S

                @arg        : str arg
                              number of bytes for the key

                @arg        : str path
                              path to file
            */

            public static string GetHash_File_B2S( string arg, string path  )
            {
                int hash_size = 64;
                Int32.TryParse( arg, out hash_size );

                using ( FileStream stream = File.OpenRead( path ) )
                {
                    byte[] bytes    = Blake2Fast.Blake2s.ComputeHash( hash_size, Helpers.FSReadFull( stream ) );
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
                Algo
            */

            static public string GetHash_Directory_CRC( string algo, string folder )
            {

                var files                   = Directory.GetFiles( folder, "*.*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );
                using (var file_total       = (HashAlgorithm)CryptoConfig.CreateFromName( algo ) )
                {
                    int bytes_read_chunk    = 2048;

                    foreach ( string file in files )
                    {
                        using (var file_single = (HashAlgorithm)CryptoConfig.CreateFromName( algo ) )
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
                MD5, SHA-2
            */

            static public string GetHash_Directory_SHA2( string algo, string folder )
            {

                var files                   = Directory.GetFiles( folder, "*.*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );
                using (var file_total       = (HashAlgorithm)CryptoConfig.CreateFromName( algo ) )
                {
                    int bytes_read_chunk    = 2048;

                    foreach ( string file in files )
                    {
                        using (var file_single = (HashAlgorithm)CryptoConfig.CreateFromName( algo ) )
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


            /*
                Blake > 2b

                @arg        : str arg
                @arg        : str folder
                @ret        : str
            */

            static public string GetHash_Directory_B2B( string arg, string folder )
            {
                int hash_size = 64;
                Int32.TryParse( arg, out hash_size );

                var files                   = Directory.GetFiles( folder, "*.*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );
                using (var file_total       = Blake2b.CreateHashAlgorithm( hash_size ) )
                {
                    int bytes_read_chunk    = 2048;

                    foreach ( string file in files )
                    {
                        using (var file_single = Blake2b.CreateHashAlgorithm( hash_size ) )
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
                Blake > 2s

                @arg        : str arg
                @arg        : str folder
                @ret        : str
            */

            static public string GetHash_Directory_B2S( string arg, string folder )
            {
                int hash_size = 64;
                Int32.TryParse( arg, out hash_size );

                var files                   = Directory.GetFiles( folder, "*.*", SearchOption.AllDirectories ).OrderBy( p => p ).ToList( );
                using (var file_total       = Blake2s.CreateHashAlgorithm( hash_size ) )
                {
                    int bytes_read_chunk    = 2048;

                    foreach ( string file in files )
                    {

                        using (var file_single = Blake2s.CreateHashAlgorithm( hash_size ) )
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
                Management > Algo

                @arg        : str algo
                @arg        : str input
                @ret        : str
            */

            public static string Hash_Manage_CRC( string algo, string input )
            {
                if ( Directory.Exists( input ) )
                    return Hash.GetHash_Directory_CRC( algo,  input );
                else if ( File.Exists( input ) )
                    return Hash.GetHash_File_CRC( algo, input );
                else
                    return Hash.GetHash_String_CRC( algo, input );
            }

            /*
                Management > MD5, SHA1, SHA-2

                @arg        : str algo
                @arg        : str input
                @ret        : str
            */

            public static string Hash_Manage_SHA2( string algo, string input )
            {
                if ( Directory.Exists( input ) )
                    return Hash.GetHash_Directory_SHA2( algo,  input );
                else if ( File.Exists( input ) )
                    return Hash.GetHash_File_SHA2( algo, input );
                else
                    return Hash.GetHash_String_SHA2( algo, input );
            }

            /*
                Management > SHA-3

                @arg        : str algo
                @arg        : str input
                @ret        : str
            */

            public static string Hash_Manage_SHA3( string algo, string input )
            {
                if ( Directory.Exists( input ) )
                    return Hash.GetHash_Directory_SHA3( algo,  input );
                else if ( File.Exists( input ) )
                    return Hash.GetHash_File_SHA3( algo, input );
                else
                    return Hash.GetHash_String_SHA3( algo, input );
            }

            /*
                Management > Blake2 > 2B

                @arg        : str arg
                @arg        : str input
                @ret        : str
            */

            public static string Hash_Manage_B2B( string arg, string input )
            {
                if ( Directory.Exists( input ) )
                    return Hash.GetHash_Directory_B2B( arg,  input );
                else if ( File.Exists( input ) )
                    return Hash.GetHash_File_B2B( arg, input );
                else
                    return Hash.GetHash_String_B2B( arg, input );
            }

            /*
                Management > Blake2 > 2S

                @arg        : str arg
                @arg        : str input
                @ret        : str
            */

            public static string Hash_Manage_B2S( string arg, string input )
            {
                if ( Directory.Exists( input ) )
                    return Hash.GetHash_Directory_B2S( arg,  input );
                else if ( File.Exists( input ) )
                    return Hash.GetHash_File_B2S( arg, input );
                else
                    return Hash.GetHash_String_B2S( arg, input );
            }

            /*
                Management > Argon 2

                @arg        : str arg
                @arg        : str salt
                @ret        : str memory
            */

            public static string Hash_Manage_AG2( string input, string salt, int memory = 1024, int len = 4, int iterations = 1, int threads = 1, int lanes = 1 )
            {

                if ( AppInfo.bIsDebug( ) )
                {
                    StackTrace stackTrace = new StackTrace( ); 
                    Console.WriteLine( stackTrace.GetFrame( 1 ).GetMethod( ).Name);
                }

                if ( Directory.Exists( input ) )
                {
                    App.MaintFeatureSoon( );
                    return "Error";
                }
                else if ( File.Exists( input ) )
                {
                    App.MaintFeatureSoon( );
                    return "Error";
                }
                else
                    return Hash.GetHash_String_AG2( input, salt, memory, len, iterations, threads, lanes );
            }


        #endregion

    }

}

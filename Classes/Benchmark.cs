#region "Using"

using XSum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Data;
using SHA3M.Security.Cryptography;
using System.Drawing;

#endregion

namespace XSum
{

    /*
        Benchmark > Buffer
        create a byte array with a bunch of nonsense data that we'll use to 
        perform our benchmark
    */

    public class BenchmarkBuffer
    {
        private readonly Random rand = new Random( );
        private readonly byte[] seed;

        public BenchmarkBuffer( int size_max_buffer )
        {
            seed = new byte[ size_max_buffer ];
            rand.NextBytes( seed );
        }

        public byte[] Generate( int size )
        {
            int next        = rand.Next( 0, size );
            byte[] buffer   = new byte[ size ];

            Buffer.BlockCopy( seed, next, buffer, 0, size - next );
            Buffer.BlockCopy( seed, 0, buffer, size - next, next );

            return buffer;
        }
    }

    /*
        Class > Benchmark
    */

    public class Benchmark
    {

        /*
            Define > File Name
            utilized with Log module.
        */

        #region "Define: Fileinfo"

            readonly static string log_file = "Benchmark.cs";

        #endregion



        /*
            Define > Classes
        */

        #region "Define: Classes"

            private AppInfo AppInfo             = new AppInfo( );
            static Helpers Helpers              = new Helpers( );

        #endregion




        /*
            Define > Delegate

            This is a console application. Short-hand some of the console methods to keep the code clean.
        */

        #region "Define: Delegates"

            delegate string del_sf          ( string format, params object[] arg );
            static del_sf       sf          = String.Format;

            delegate void del_write         ( string format, params object[] arg );
            static del_write    wl          = Console.WriteLine;
            static del_write    ws          = Console.Write;
            static Action       rs          = ( ) => Console.ForegroundColor = ConsoleColor.White;

        #endregion



        /*
            Methods: Shorthand Helpers
        */

        #region "Methods: Shorthand"

            /*
                Define > Console New Line
            */

            static public void nl( int lines = 1 )
            {
                Console.ForegroundColor = ConsoleColor.White;
                for ( int i = 0; i < lines; i++ )
                {
                    Console.WriteLine( );
                }
            }

            /*
                Define > Console fg
            */

            static public void fg( ConsoleColor clr )
            {
                Console.ForegroundColor = clr;
            }


            /*
                Helpers > Console Colors ( v1 )
                Allows you to print output to the console using a mixture of colors and strings.

                @usage      : c1( "this is white ", ConsoleColor.Red, "this is red", null, " and back to white and ", ConsoleColor.Green, "now to green", null, " and finally white" );
                              c1( "this is white ", ConsoleColor.Red, "this is red", "#", " and back to white and ", ConsoleColor.Green, "now to green", "#", " and finally white" );
             */

            static private void c1( params object[] msg )
            {
                Console.ForegroundColor = ConsoleColor.White;

                foreach( var resp in msg )
                {
                    if ( resp == null || ( resp is string && resp.ToString( ) == "#" ) )
                        Console.ResetColor( );
                    else if ( resp is ConsoleColor )
                        Console.ForegroundColor = (ConsoleColor)resp;
                    else
                        Console.Write( resp.ToString( ) );
                }

                Console.ForegroundColor = ConsoleColor.White;
            }

            /*
                Helpers > Console Colors ( v2 )
                Allows you to print output to the console using a mixture of colors and strings.

                @usage      : c2( "this is white [#Red]this in red[/] and {#Yellow}this is yellow[/] now white" );
            */

            static private void c2( string msg )
            {
                string[] str        = msg.Split( '[',']' );
                ConsoleColor        clr;

                Console.ForegroundColor = ConsoleColor.White;

                foreach( var resp in str )
                {
                    if( resp.StartsWith( "/" ) )
                        Console.ResetColor( );
                    else if( resp.StartsWith( "#" ) && Enum.TryParse( resp.Substring( 1 ), out clr ) )
                        Console.ForegroundColor = clr;
                    else
                        Console.Write( resp );
                }

                Console.ForegroundColor = ConsoleColor.White;
            }

            static private void c0( string msg )
            {
                string[] str        = msg.Split( '<','>' );
                ConsoleColor        clr;

                Console.ForegroundColor = ConsoleColor.White;

                foreach( var resp in str )
                {
                    if( resp.StartsWith( "/" ) )
                        Console.ResetColor( );
                    else if( resp.StartsWith( "#" ) && Enum.TryParse( resp.Substring( 1 ), out clr ) )
                        Console.ForegroundColor = clr;
                    else
                        Console.Write( resp );
                }

                Console.ForegroundColor = ConsoleColor.White;
            }

            static public ConsoleColor wc( string ColorName )
            {
                ConsoleColor consoleColor;

                if (Enum.TryParse( ColorName, out consoleColor ) )
                {
                    Console.ForegroundColor = consoleColor;
                    return consoleColor;
                }

                //do whatever you want if it's invalid ColorName    
                Console.WriteLine( "invalid color" );

                return ConsoleColor.White;
            }

        #endregion



        /*
            Define > Byte Size Suffixes
        */

        #region "Define: File Size Suffix"

            static readonly string[] suffix_size_file       = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            static readonly string[] suffix_size_speed      = { "KiB/s", "MiB/s", "GiB/s", "TiB/s", "PiB/s", "EiB/s", "ZiB/s", "YiB/s" };

        #endregion



        /*
            Format > Suffix > Bytes Conversion

            Converts bytes to KB, MB, GB, etc.

            @arg        : int bytes
                          byte size of item

            @arg        : int dec
                          decimal places

            @ret        : str
        */

        #region "Method: Suffix - Byte Size"

            static string Format_Suffix_Bytes_Size( Int64 bytes, int dec = 2 )
            {

                if ( dec < 0 )              { throw new ArgumentOutOfRangeException( "dec" ); }
                if ( bytes < 0 )            { return "∞"; } 
                if ( bytes == 0 )           { return string.Format( "{0:n" + dec + "}", 0 ); }

                int log                     = (int)Math.Log( bytes, 1024 );
                decimal file_size           = (decimal)bytes / ( 1L << ( log * 10 ) );

                if ( Math.Round( file_size, dec ) >= 1024 )
                {
                    log += 1;
                    file_size /= 1024;
                }

                return string.Format( "{0:n" + dec + "} {1}", file_size, suffix_size_file[ log ] );
            }

        #endregion



        /*
            Suffix > Bytes to Speed

            Calculates the speed of a task into:
                - B/s
                - KiB/s
                - MiB/s
                - {...}

            @arg        : bytes bytes
                          byte size of item

            @arg        : stopwatch time
                          decimal places

            @arg        : int dec
                          decimal places

            @ret        : str
        */

        #region "Method: Suffix - Byte Speed"

            static string Format_Suffix_Bytes_Speed( byte[] bytes, Stopwatch time, int dec = 2 )
            {
                double init_time            = ( (double)bytes.Length / 1.024 / time.ElapsedMilliseconds );
                Int64 value                 = (long)init_time;

                if ( dec < 0 )              { throw new ArgumentOutOfRangeException( "dec" ); }
                if ( value < 0 )            { return "∞"; } 
                if ( value == 0 )           { return string.Format( "{0:n" + dec + "} B/s", 0 ); }

                int log                     = (int)Math.Log( value, 1024 );
                decimal size_speed          = (decimal)value / ( 1L << ( log * 10 ) );

                if ( Math.Round( size_speed, dec ) >= 1024 )
                {
                    log += 1;
                    size_speed /= 1024;
                }

                return string.Format( "{0:n" + dec + "} {1}", size_speed, suffix_size_speed[ log ] );
            }

        #endregion



        /*
            Format > Time

            Takes stopwatch time and formats it to display for user.
        */

        #region "Method: Format Time"

            static public string Format_Time( Stopwatch timer )
            {

                double time   = timer.Elapsed.TotalMilliseconds;
    
                if ( time > 1000 )
                {
                    double seconds = (double)time / 1000;
                    return seconds.ToString( "0.00" ) + "s";
                }
                else if ( time < 1 )
                {
                    return time.ToString( "∞" );
                }

                return timer.Elapsed.TotalMilliseconds.ToString( "0.00" ) + "ms";
            }

        #endregion



        /*
            Format > Response

            Format results to add default values
        */

        #region "Method: Format Response"

            static public string Format_Response( string response, bool bNull = false )
            {
                if ( String.IsNullOrEmpty( response ) || bNull )
                    return "-";

                return response;
            }

        #endregion



        /*
            Benchmark > Start

            @arg        : int buffer
                          size of buffer to test with
            @def        : 32000000 (32 Metabytes)

            @arg        : int iterations
                          number of iterations
            @def        : 50

        */

        static public void Start( int buffer = 32000000, int iterations = 50 )
        {

            #region "Method: Start -> Create SHA Instance"

                /*
                    SHA Instance > Unmanaged
                */

                MD5Cng          umg_md5         = new MD5Cng( );
                SHA1Cng         umg_sha1        = new SHA1Cng( );
                SHA256Cng       umg_sha256      = new SHA256Cng( );
                SHA384Cng       umg_sha384      = new SHA384Cng( );
                SHA512Cng       umg_sha512      = new SHA512Cng( );

                /*
                    SHA Instance > Managed
                */

                MD5             mng_md5         = null;
                SHA1Managed     mng_sha1        = new SHA1Managed( );
                SHA256Managed   mng_sha256      = new SHA256Managed( );
                SHA384Managed   mng_sha384      = new SHA384Managed( );
                SHA512Managed   mng_sha512      = new SHA512Managed( );

                /*
                    SHA Instance > CSP ( CryptoServiceProvider )
                */

                MD5CryptoServiceProvider      csp_md5         = new MD5CryptoServiceProvider( );
                SHA1CryptoServiceProvider     csp_sha1        = new SHA1CryptoServiceProvider( );
                SHA256CryptoServiceProvider   csp_sha256      = new SHA256CryptoServiceProvider( );
                SHA384CryptoServiceProvider   csp_sha384      = new SHA384CryptoServiceProvider( );
                SHA512CryptoServiceProvider   csp_sha512      = new SHA512CryptoServiceProvider( );

                /*
                    SHA Instance > Create
                */

                MD5             crt_md5         = MD5.Create( );
                SHA1            crt_sha1        = SHA1.Create( );
                SHA256          crt_sha256      = SHA256.Create( );
                SHA384          crt_sha384      = SHA384.Create( );
                SHA512          crt_sha512      = SHA512.Create( );

            #endregion



            /*
                Generate buffer bytes for test
            */

            BenchmarkBuffer buffer_new  = new BenchmarkBuffer   ( buffer );
            byte[] bench_buffer         = buffer_new.Generate   ( buffer );
            byte[] data_bytes           = bench_buffer;

            /*
                Create Data Columns
            */

            int col_run                 = 3;
            int col_algo                = 7;
            int col_type                = 19;
            int col_dur                 = 9;
            int col_speed               = 16;
            int col_result              = 20;


            string header_arch          = ( ( IntPtr.Size == 8 ) ? "64" : "32" ) + "-bit";
            string header_title         = String.Format( "Benchmark ( {0} )", header_arch );
            int header_size             = col_run + col_algo + col_type + col_dur + col_speed + col_result + 12;

            /*
                Create Data Table > Header
            */

            Console.ForegroundColor     = ConsoleColor.Green;

            DataTable dt_header         = new DataTable( "" );
            dt_header.Columns.Add       ( Helpers.ALC( header_title, header_size ), typeof(string) );
            dt_header.Rows.Add          ( Helpers.ALC( "Buffer Size: " + Format_Suffix_Bytes_Size( buffer ).ToString( ) + " | Iterations: " + iterations.ToString( "N0" ), header_size ) );
            TableStyle.BorderNone       ( );
            dt_header.Print             ( );
            TableStyle.BorderNormal     ( );

            Console.ForegroundColor     = ConsoleColor.White;

            /*
                Stopwatch
            */

            Stopwatch timer             = new Stopwatch( );

            /*
                Create Data Table > First Run
            */

            DataTable dt_rnd1           = new DataTable( "" );

            dt_rnd1.Columns.Add( Helpers.ALC( "Run",       col_run     ),  typeof(string) );
            dt_rnd1.Columns.Add( Helpers.ALR( "Algo",      col_algo    ),  typeof(string) );
            dt_rnd1.Columns.Add( Helpers.ALR( "Type",      col_type    ),  typeof(string) );
            dt_rnd1.Columns.Add( Helpers.ALL( "Duration",  col_dur     ),  typeof(string) );
            dt_rnd1.Columns.Add( Helpers.ALL( "Speed",     col_speed   ),  typeof(string) );
            dt_rnd1.Columns.Add( Helpers.ALL( "Result",    col_result  ),  typeof(string) );

            /*
                Define > Algs
            */

            string alg_md5              = "MD5";
            string alg_sha1             = "SHA1";
            string alg_sha256           = "SHA256";
            string alg_sha384           = "SHA384";
            string alg_sha512           = "SHA512";
            string alg_sha3256          = "SHA3-512";

            /*
                Define > Defaults
            */

            int dt_round                = 1;
            string response             = null;

            /*
                First Run > MD5
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            //response          = Hash.BytesToString( mng_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response, true ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_md5, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            dt_rnd1.Rows.Add    ( "", "", "", "", "", ""  );

            /*
                First Run > SHA1
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha1, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            dt_rnd1.Rows.Add    ( "", "", "", "", "", ""  );

            /*
                First Run > SHA256
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha256, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            dt_rnd1.Rows.Add    ( "", "", "", "", "", ""  );

            /*
                First Run > SHA386
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha384, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( response, col_result ) );

            dt_rnd1.Rows.Add    ( "", "", "", "", "", ""  );

            /*
                First Run > SHA512
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha512, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd1.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            /*
                Show first table
            */

            Console.ForegroundColor = ConsoleColor.Yellow;
            dt_rnd1.Print( );
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine( );


            /*
                Create Data Table > Row 2
            */

            Console.ForegroundColor     = ConsoleColor.Green;

            DataTable dt_mng_hdr        = new DataTable( "" );
            dt_mng_hdr.Columns.Add      ( Helpers.ALC( header_title, header_size ), typeof(string) );
            dt_mng_hdr.Rows.Add         ( Helpers.ALC( "Buffer Size: " + Format_Suffix_Bytes_Size( buffer ).ToString( ) + " | Iterations: " + iterations.ToString( "N0" ) , header_size ) );
            TableStyle.BorderNone       ( );
            dt_mng_hdr.Print            ( );
            TableStyle.BorderNormal     ( );

            Console.ForegroundColor     = ConsoleColor.White;

            /*
                Create Data Table > Second Run
            */

            DataTable dt_rnd2       = new DataTable( "" );
            dt_round                = 2;

            dt_rnd2.Columns.Add( Helpers.ALC( "Run",         col_run    ),       typeof(string) );
            dt_rnd2.Columns.Add( Helpers.ALC( "Algo",        col_algo   ),       typeof(string) );
            dt_rnd2.Columns.Add( Helpers.ALR( "Type",        col_type   ),       typeof(string) );
            dt_rnd2.Columns.Add( Helpers.ALL( "Duration",    col_dur    ),       typeof(string) );
            dt_rnd2.Columns.Add( Helpers.ALL( "Speed",       col_speed  ),       typeof(string) );
            dt_rnd2.Columns.Add( Helpers.ALL( "Result",      col_result  ),      typeof(string) );

            /*
                Final Run > MD5
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            //response          = Hash.BytesToString( mng_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response, true ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_md5.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_md5, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_md5, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            dt_rnd2.Rows.Add    ( "", "", "", "", "", ""  );


            /*
                First Run > SHA1
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha1.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha1, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha1, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            dt_rnd2.Rows.Add    ( "", "", "", "", "", ""  );

            /*
                First Run > SHA256
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha256.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha256, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha256, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            dt_rnd2.Rows.Add    ( "", "", "", "", "", ""  );

            /*
                First Run > SHA386
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha384.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha384, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha384, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( response, col_result ) );

            dt_rnd2.Rows.Add    ( "", "", "", "", "", ""  );

            /*
                First Run > SHA512
            */

            timer.Restart       ( );
            response            = Hash.BytesToString( umg_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "Unmanaged", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( mng_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "Managed", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( csp_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "CSP", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( crt_sha512.ComputeHash( data_bytes ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( "Create", col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            timer.Restart       ( );
            response            = Hash.BytesToString( Hash.Compute( alg_sha512, data_bytes, iterations ) );
            timer.Stop          ( );
            dt_rnd2.Rows.Add    ( Helpers.ALC( dt_round.ToString( ), col_run ), Helpers.ALR( alg_sha512, col_algo ), Helpers.ALR( String.Format( "Iterations x{0}", iterations.ToString( ) ), col_type ), Helpers.ALL( Format_Time( timer ), col_dur ), Helpers.ALL( Format_Suffix_Bytes_Speed( data_bytes, timer ), col_speed ), Helpers.ALL( Format_Response( response ), col_result ) );

            /*
                Show Second Table
            */

            Console.ForegroundColor = ConsoleColor.Red;
            dt_rnd2.Print( );
            Console.ForegroundColor = ConsoleColor.White;

        }

        /*
            Benchmark > Algorithm Stress Test
        */

        static public void Perform_Algo_Stress( string algo = "sha1", int iterations = 100000 )
        {

            /*
                Character Test > Character Iterations
                do not set this amount too high, it will cause massive performance issues

                calling this via command-line with --iterations instead of --characters because it would be
                harder for people to understand.

                @default        : 100000
            */

            iterations                  = ( iterations == 0 ? 100000 : iterations );
            int n                       = iterations;

            /*
                Character Test > Table Column Widths
            */

            int col_run             = 3;
            int col_algo            = 7;
            int col_char            = 8;
            int col_avg             = 11;
            int col_elap            = 11;
            int col_speed           = 18;
            int col_result          = 20;

            /*
                Warn user of high iteration counts
            */

            if ( iterations > 50000 )
            {
                nl( );
                c2( sf( " {0,-26} {1,-30}", "[#Yellow]Warning[/]", "Specified over [#DarkYellow]50,000 iterations[/], this could take upwards of a minute to complete.[/]" ) );
                nl( );
            }

            /*
                Character Test > Table Total Width
            */

            int header_width        = col_run + col_algo + col_char + col_avg + col_elap + col_speed + col_result + 12;
            string header_name      = n.ToString( "N0" );

            /*
                Character Test > Generate Words

                these instructions will generate a bunch of words that will be used to perform these tests.
            */

            Console.WriteLine( "\n" );

            int words_gen               = (int)n;                                       // Generate percentage of words based on character count
            string wordlist             = Helpers.RandomString( words_gen, true );      // Generate words

            Console.ForegroundColor     = ConsoleColor.Green;
            DataTable dt_hdr            = new DataTable( "" );
            dt_hdr.Columns.Add          ( Helpers.ALC( String.Format( "{0} Benchmark", algo.ToUpper( ) ), header_width ), typeof(string) );
            dt_hdr.Rows.Add             ( Helpers.ALC( "Iteration: " + header_name, header_width )  );
            TableStyle.BorderNone       ( );
            dt_hdr.Print                ( );
            TableStyle.BorderNormal     ( );
            Console.ForegroundColor     = ConsoleColor.White;

            /*
                Character Test > Column Sizes
            */

            DataTable dt_chr            = new DataTable( "" );
            dt_chr.Columns.Add          ( Helpers.ALC( "Algo",        col_algo      ),  typeof(string) );
            dt_chr.Columns.Add          ( Helpers.ALR( "Chars",       col_char      ),  typeof(string) );
            dt_chr.Columns.Add          ( Helpers.ALR( "Elapsed",     col_elap      ),  typeof(string) );
            dt_chr.Columns.Add          ( Helpers.ALL( "Average",     col_avg       ),  typeof(string) );
            dt_chr.Columns.Add          ( Helpers.ALL( "Speed",       col_speed     ),  typeof(string) );
            dt_chr.Columns.Add          ( Helpers.ALL( "Result",      col_result    ),  typeof(string) );

            /*
                Character Test > Stopwatch > Create
            */

            Stopwatch sw            = new Stopwatch();

            /*
                Character Test > Test Instructions
                
                create the test instructions.
                each step performs a percantage of the overall iterations specified above.

                ( iterations    * percentage    ) / 100
                ( 1000          * 0.5           ) / 100 = 50%
            */

            double calc_step_1      = Math.Round( ( n * 0.005 ) / 100 );    // 5
            double calc_step_2      = Math.Round( ( n * 0.010 ) / 100 );    // 10
            double calc_step_3      = Math.Round( ( n * 0.050 ) / 100 );    // 50
            double calc_step_4      = Math.Round( ( n * 0.100 ) / 100 );    // 100
            double calc_step_5      = Math.Round( ( n * 0.200 ) / 100 );    // 200
            double calc_step_6      = Math.Round( ( n * 0.500 ) / 100 );    // 500
            double calc_step_7      = Math.Round( ( n * 1.000 ) / 100 );    // 1000
            double calc_step_8      = Math.Round( ( n * 2.000 ) / 100 );    // 2000
            double calc_step_9      = Math.Round( ( n * 5.000 ) / 100 );    // 5000
            double calc_step_10     = Math.Round( ( n * 10.000 ) / 100 );   // 10000
            double calc_step_11     = Math.Round( ( n * 50.000 ) / 100 );   // 50000
            double calc_step_12     = Math.Round( ( n * 100.000 ) / 100 );  // 100000

            /*
                Character Test > Create Substrings

                take the overall string of words and break them up for the test.
                each step will gradually include more.
            */

            string[] input      = new string[]
            {
                wordlist.Substring( 0, (int)calc_step_1 ),
                wordlist.Substring( 0, (int)calc_step_2 ),
                wordlist.Substring( 0, (int)calc_step_3 ),
                wordlist.Substring( 0, (int)calc_step_4 ),
                wordlist.Substring( 0, (int)calc_step_5 ),
                wordlist.Substring( 0, (int)calc_step_6 ),
                wordlist.Substring( 0, (int)calc_step_7 ),
                wordlist.Substring( 0, (int)calc_step_8 ),
                wordlist.Substring( 0, (int)calc_step_9 ),
                wordlist.Substring( 0, (int)calc_step_10 ),
                wordlist.Substring( 0, (int)calc_step_11 ),
                wordlist.Substring( 0, (int)calc_step_12 ),
            };

            /*
                Loop characters
            */

            byte[] hash = null;

            bool bIsSHA3 = algo.Substring( 0, 4 ).Contains("sha3");

            for ( int i = 0; i < input.Length; i++ )
            {
                sw.Restart( );

                for ( int k = 0; k < n; k++ )
                {
                    HashAlgorithm hasher    = ( HashAlgorithm)CryptoConfig.CreateFromName( algo );

                    if ( bIsSHA3 )
                        hasher              = ( HashAlgorithm)SHA3.Create( algo );

                    hash                    = hasher.ComputeHash( Encoding.ASCII.GetBytes( input[ i ] ) );
                }

                sw.Stop();

                double time_micro   = ( 1000 * ( sw.Elapsed.TotalMilliseconds / n ) );
                double time_ms      = ( (double)n / sw.Elapsed.TotalMilliseconds );

                string stats_char   = String.Format( "{0}", input[ i ].Length );
                string stats_elap   = Format_Time( sw );
                string stats_avg    = String.Format( "{0} µs", time_micro.ToString( "0.00" ) );
                string stats_speed  = String.Format( "{0} calls/ms", time_ms.ToString( "0.00" ) );
                string stats_resp   = Hash.BytesToString( hash );

                /*
                    Add Table Row
                */

                dt_chr.Rows.Add     ( Helpers.ALR( algo.ToUpper( ), col_algo ), Helpers.ALR( stats_char, col_char ), Helpers.ALR( stats_elap, col_elap ), Helpers.ALL( stats_avg, col_avg ), Helpers.ALL( stats_speed, col_speed ), Helpers.ALL( Format_Response( stats_resp ), col_result ) );

            }

            /*
                Show Second Table
            */

            Console.ForegroundColor = ConsoleColor.Green;
            dt_chr.Print( );
            Console.ForegroundColor = ConsoleColor.White;

        }

    }
}

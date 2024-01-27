﻿#region "Using"

using XSum;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Management.Automation;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using xsum;
using System.Windows;
using Cfg = xsum.Properties.Settings;
using System.Collections;
using System.Management.Automation.Language;
using System.Xml.Linq;
using System.Net.Security;

#endregion

namespace XSum
{


    public class App
    {

        /*
            Define: Paths
            lists the default paths associated with this app.
        */

        #region "Define: Paths"

            static string xsum_path_full        = Process.GetCurrentProcess( ).MainModule.FileName;     // XSum full path to exe
            static string xsum_path_dir         = Path.GetDirectoryName( xsum_path_full );              // XSum path to folder only
            static string xsum_path_exe         = Path.GetFileName( xsum_path_full );                   // XSum exe name only

        #endregion



        /*
            Define > File Name
            utilized with Log module.
        */

        #region "Define: Fileinfo"

            readonly static string log_file = "App.cs";

        #endregion



        /*
            Define > Classes
        */

        #region "Define: Classes"

            private AppInfo AppInfo             = new AppInfo( );
            private Benchmark Benchmark         = new Benchmark( );

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
            Define > Misc
        */

        #region "Define: Miscellaneous"

            static string assemblyName      = Assembly.GetEntryAssembly( ).GetName( ).Name;

        #endregion



        /*
            Define > Exit Codes

            None, Abort, Success, ErrorMissingArg, ErrorMissingDep, ErrorGeneric
        */

        #region "ENUM: Exit Codes"

            [Flags]
            enum ExitCode : int
            {
                None                = -1,
                Abort               = 0,
                Success             = 1,
                ErrorMissingArg     = 2,
                ErrorMissingDep     = 4,
                ErrorGeneric        = 8,
            }

        #endregion




        /*
            Execute powershell query
            checks to see if a target file has been signed with x509 cert

            @param      : str query
            @return     : str
        */

        #region "Method: Exec Powershell Queries"

            public string PowershellQ( string[] queries )
            {
                using ( PowerShell ps = PowerShell.Create( ) )
                {
                    for ( int i = 0; i < queries.Length; i++ ) 
                    {
                        ps.AddScript    ( queries[ i ] );
                        Log.Send        ( log_file, new System.Diagnostics.StackTrace( true ).GetFrame( 0 ).GetFileLineNumber( ), "[ PSQ.Execute ]", String.Format( "{0}", queries[ i ] ) );
                    }

                    Collection<PSObject> PSOutput = ps.Invoke( );
                    StringBuilder sb = new StringBuilder( );

                    string resp = "";
                    foreach ( PSObject PSItem in PSOutput )
                    {
                        if ( PSItem != null )
                        {
                            Log.Send( log_file, new System.Diagnostics.StackTrace( true ).GetFrame( 0 ).GetFileLineNumber( ), "[ PSQ.Result ]", String.Format( "{0}", PSItem ) );
                            sb.AppendLine( PSItem.ToString( ) );
                        }
                    }

                    if ( ps.Streams.Error.Count > 0 )
                    {
                        resp += Environment.NewLine + string.Format( "{0} Error(s): ", ps.Streams.Error.Count );
                        foreach ( ErrorRecord err in ps.Streams.Error )
                                resp += Environment.NewLine + err.ToString();

                        Log.Send( log_file, new System.Diagnostics.StackTrace( true ).GetFrame( 0 ).GetFileLineNumber( ), "[ PSQ.Error ]", String.Format( "{0}", resp ) );
                    }

                    return sb.ToString( );
                }
            }

        #endregion



        /*
            Find Program

            Reads the Windows path entries and checks to see if a specified exe exists.
        */

        #region "Method: FindProgram"

            /*
                Route Terminal Output
                Hacky method for keeping console commands from printing re-route output

                Utilized in combination with Method FindProgram.
            */

            static public void RouteOutput( string input ) { }

            public static bool FindProgram( string appexe )
            {
                try
                {
                    using ( Process p = new Process( ) )
                    {
                        p.StartInfo.UseShellExecute         = false;
                        p.StartInfo.FileName                = "where";
                        p.StartInfo.Arguments               = appexe;
                        p.StartInfo.RedirectStandardOutput  = true;
                        p.OutputDataReceived                += (s, e) => RouteOutput( e.Data );
                        p.Start( );
                        p.BeginOutputReadLine( );
                        p.WaitForExit( );

                        return p.ExitCode == 0;
                    }
                }
                catch( Win32Exception )
                {
                    throw new Exception( "'where' command is not on path" );
                }
            }

        #endregion



        /*
            Helpers > Check Full Path
        */

        #region "Method: Path > Check Full"

            /*
                Helpers > Path > Check Rooted / Full Path
                returns if a specified path is root.
                Utilized for output directories.
            */

            public static bool IsFullPath( string path )
            {
                return !String.IsNullOrWhiteSpace( path )
                    && path.IndexOfAny( System.IO.Path.GetInvalidPathChars( ).ToArray( ) ) == -1
                    && Path.IsPathRooted( path )
                    && !Path.GetPathRoot( path ).Equals(Path.DirectorySeparatorChar.ToString( ), StringComparison.Ordinal );
            }

        #endregion



        /*
            Define > Global Settings
        */

        #region "Global Settings"

            static bool     arg_LC_Enabled          = false;                        // Enable hash lowercase
            static bool     arg_GenMode_Enabled     = false;                        // --generate arg specified
            static bool     arg_VerifyMode_Enabled  = false;                        // --verify arg specified
            static bool     arg_SignMode_Enabled    = false;                        // --sign arg specified

            static bool     arg_Target_Enabled      = false;                        // --target specified
            static string   arg_Target_File         = null;                         // --verify <FILE> || --target <FILE> location
            static bool     arg_Progress_Enabled    = false;                        // --verify --progress specified
            static bool     arg_Digest_Enabled      = false;                        // --digest arg specified
            static string   arg_Digest_File         = null;                         // --digest <FILE> location
            static bool     arg_Debug_Enabled       = false;                        // --debug arg specified
            static bool     arg_Overwrite_Enabled   = false;                        // --overwrite arg specified
            static bool     arg_Clipboard_Enabled   = false;                        // --clipboard arg specified
            static string   arg_Algo_List           = Hash_GetSupported( );         // list of supported algorithms
            static bool     arg_Output_Enabled      = false;                        // --output arg specified
            static string   arg_Output_File         = null;                         // --output <FILE> location
            static bool     arg_Benchmark_Enabled   = false;                        // --benchmark arg specified
            static bool     arg_Iterations_Enabled  = false;                        // --iterations arg specified
            static int      arg_Iterations_Min      = 1;                            // --iterations min value
            static int      arg_Iterations_Max      = 500000;                       // --iterations max value
            static int      arg_Iterations          = 0;                            // --iterations number
            static bool     arg_Buffer_Enabled      = false;                        // --buffer arg specified
            static int      arg_Buffer              = 0;                            // --buffer number
            static int      arg_Buffer_Min          = 5242880;                      // --buffer min value
            static int      arg_Buffer_Max          = 512000000;                    // --buffer max value
            static bool     arg_Algo_Enabled        = false;                        // --algo arg specified
            static string   arg_Algo_Default        = "sha256";                     // --algo default value
            static string   arg_Algo                = arg_Algo_Default;             // --algo <ALG> specified

        #endregion

        /*
            Help Dialog
                default if no arguments are specified by user
        */

        #region "Method: Help Menu"

        static public int Help( )
        {

                nl( );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Red );
                nl( );
                wl( " " + AppInfo.Title + " " );
                nl( );
                wl( " @author        : " + AppInfo.Company + " " );
                wl( " @version       : " + AppInfo.PublishVersion + " " );
                wl( " @copyright     : " + AppInfo.Copyright + " " );
                wl( " @website       : https://github.com/aetherinox/XSum" );
                nl( );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Gray );
                nl( );
                wl(@" XSum.exe is a checksum utility.

 Command-line utility for Windows which generates and verifies the hash of 
 directories and files. If an invalid file or folder is provided, xsum will
 treat it as a normal string which will be hashed.

 Hash digests can be signed utilizing GPG.
                ");
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.Gray );
                nl( );

                wl( " Usage:" );
                nl( );

                c0( sf( "   <#Yellow>{0} --generate --target [ DIR|FILE|STRING ] [ --digest HASH_FILE ] </>\n", xsum_path_exe ) );
                c0( sf( "          <#Yellow>[ --algo MD5|SHA1|SHA256|SHA384|SHA512 ] [ --output FILE ] </>\n" ) );
                c0( sf( "          <#Yellow>[ --lowercase ] [ --overwrite ] [ --progress ] [--clip]</>\n" ) );

                nl( );

                c0( sf( "   <#Yellow>{0} --verify --target [ DIR|FILE|STRING ] [ --digest HASH_FILE ]</>\n", xsum_path_exe ) );
                c0( sf( "          <#Yellow>[ --algo MD5|SHA1|SHA256|SHA384|SHA512 ] [ --output FILE ]</>\n" ) );
                c0( sf( "          <#Yellow>[ --lowercase ] [ --overwrite ] [ --progress ] [--clip]</>\n" ) );

                nl( );

                c0( sf( "   <#Yellow>{0} --benchmark [ --buffer SIZE ] [ --iterations NUMBER ]</>\n", xsum_path_exe ) );
                c0( sf( "   <#Yellow>{0} --benchmark [ --algo MD5|SHA1|SHA256|SHA384|SHA512 ] [ --iterations NUMBER ]</>\n", xsum_path_exe ) );
                c0( sf( "   <#Yellow>{0} --benchmark [ --buffer 32000000 ] [ --iterations NUMBER ]</>\n", xsum_path_exe ) );

                nl( );

                wl( " Commands:" );

                nl( );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-g, --generate[/]", "[#Gray]Compute hash for folder, files, or strings and generate new hash digest[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-v, --verify[/]", "[#Gray]Verify specified hash digset with files in path[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-s, --sign[/]", "[#Gray]Sign an existing diget with a GPG keypair[/]\n" ) );

                nl( );
                c0( " <#Yellow>--verify</>" );
                nl( 2 );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-t, --target[/]", "[#Gray]Target folder or file to generate / verify hash for[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-d, --digest[/]", "[#Gray]Hash digest which contains list of generated hashes[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-a, --algorithm[/]", "[#Gray]Algorithm used to verify --digest[/]\n" ) );
                c2( sf( " {0,1} {1,-20} {2,34}", " ", " ", "[#Gray]Default: " + arg_Algo_Default + "[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-o, --output[/]", "[#Gray]Output file for results verified[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-w, --overwrite[/]", "[#Gray]Overwrite results to --output instead of append[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-p, --progress[/]", "[#Gray]Displays each file being checked and additional info[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-l, --lowercase[/]", "[#Gray]Match and output hash value(s) in lower case instead of upper case.[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-c, --clipboard[/]", "[#Gray]Copies the output hash value to clipboard.[/]\n" ) );
                c2( sf( " {0,1} {1,-33} {2,30}", "", "[#Red]-b, --benchmark[/]", "[#Gray]Performs benchmarks on a specified algorithm or all.[/]\n" ) );

                wl( "" );
                wl( "" );
                fg( ConsoleColor.DarkRed );
                wl( " ------------------------------------------------------------------------------" );
                fg( ConsoleColor.DarkGray );
                wl( "" );
                Console.ResetColor( ) ;

            return (int)ExitCode.None;
        }

        #endregion



        /*
            Dictionary > Hashes

            - Populate Dictionary
            - Get Supported Hash List
        */

        #region "Dictionary: Hashes"

            /*
                Dictionary > Populate Hashes
            */

            static IDictionary<string, Func<string, string>> Dict_Hashes_Populate( )
            {
                var dict_GetHash        = new Dictionary<string, Func<string, string>>( );
                dict_GetHash.Add        ( "md5",    ( p ) => Hash.GetHash_MD5       ( p ) );
                dict_GetHash.Add        ( "sha1",   ( p ) => Hash.GetHash_SHA1      ( p ) );
                dict_GetHash.Add        ( "sha256", ( p ) => Hash.GetHash_SHA2_256  ( p ) );
                dict_GetHash.Add        ( "sha384", ( p ) => Hash.GetHash_SHA2_384  ( p ) );
                dict_GetHash.Add        ( "sha512", ( p ) => Hash.GetHash_SHA2_512  ( p ) );

                return dict_GetHash;
            }

            /*
                Dictionary > Get Hash Supported List
            */

            static string Hash_GetSupported( )
            {
                string str_lst          = "";
                var dict_GetHash        = Dict_Hashes_Populate( );

                StringBuilder sb        = new StringBuilder( );

                int lst_i_cur           = 0;
                int lst_i_max           = dict_GetHash.Keys.Count;

                foreach ( string file in dict_GetHash.Keys )
                {
                    lst_i_cur++;
                    sb.Append( ( lst_i_cur == lst_i_max ) ? file : file + ", " );
                    str_lst = sb.ToString( );
                }

                return str_lst;
            }

        #endregion



        /*
            Main
        */

        #region "Method: Main"

            static int Main( string[] args )
            {

                /*
                    No arguments supplied by user
                    return --help
                */

                if ( args.Length == 0 )
                {
                    Help( );
                    return (int)ExitCode.None;
                }

                var inputFile               = new List<string>( );

                /*
                    Arguments specified
                */

                if ( args.Length > 0 )
                {

                    #region "Command-line Parameters"

                        for ( int i = 0; i < args.Length; i++ )
                        {

                            /*
                                Argument convert to lowercase string
                            */

                            string a = args[ i ].ToLower( );

                            switch ( a )
                            {

                                /*
                                    CASE > HELP
                                    Display help menu.
                                */

                                case "--help":
                                case "-h":
                                case "/?":
                                case "?":
                                    Help( );
                                    return (int)ExitCode.None;

                                /*
                                    CASE > GENERATE

                                    generate new hash digest
                                */

                                case "--generate":
                                case "-g":
                                    if ( arg_VerifyMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Cannot use [#Yellow]--generate[/] and [#Yellow]--verify[/] together[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorGeneric;
                                    }

                                    arg_GenMode_Enabled = true;

                                    for ( i = i + 1; i < args.Length ; i++ )
                                    {

                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Target_File     = property;
                                    }

                                    break;

                                /*
                                    CASE > VERIFY

                                    Enable verify mode. Specify --digest <HASH_FILE> to compare the target
                                    folder with the entries in the hash digest file.
                                */

                                case "--verify":
                                case "-v":
                                    if ( arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Cannot use [#Yellow]--generate[/] and [#Yellow]--verify[/] together[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorGeneric;
                                    }

                                    arg_VerifyMode_Enabled  = true;

                                    for ( i = i + 1; i < args.Length ; i++ )
                                    {

                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Target_File     = property;
                                    }

                                    break;

                                case "--target":
                                case "-t":
                                case "--source":
                                case "-s":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/] or [#Yellow]--verify[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    for ( i = i + 1; i < args.Length ; i++ )
                                    {

                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Target_File     = property;
                                    }

                                    break;

                                /*
                                    CASE > Verbose

                                    Enable verify mode. Specify --digest <HASH_FILE> to compare the target
                                    folder with the entries in the hash digest file.

                                    Details mode lists all the files being checked and each hash.
                                */

                                case "--progress":
                                case "-p":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/] or [#Yellow]--verify[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Progress_Enabled    = true;
                                    for ( i = i + 1; i < args.Length ; i++ )
                                    {

                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Target_File     = property;
                                    }

                                    break;

                                /*
                                    CASE > SIGN

                                    Enable verify mode. Specify --digest <HASH_FILE> to compare the target
                                    folder with the entries in the hash digest file.

                                    Details mode lists all the files being checked and each hash.
                                */

                                case "--sign":

                                    arg_SignMode_Enabled    = true;

                                    for ( i = i + 1; i < args.Length ; i++ )
                                    {

                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Target_File     = property;
                                    }

                                    break;

                                /*
                                    CASE > DIGEST

                                    Specify digest file to compare project files to.
                                    Digest file contains the hash for the file, and the path to the file.
                                */

                                case "--digest":
                                case "-d":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/] or [#Yellow]--verify[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Digest_Enabled      = true;

                                    for ( i = i + 1; i < args.Length ; i++ )
                                    {

                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Digest_File     = property;
                                    }

                                    if ( arg_Digest_Enabled && String.IsNullOrEmpty( arg_Digest_File ) )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--digest specified but missing [#Yellow]--digest <FILE_HASH_DIGEST>[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    break;

                                /*
                                    CASE > ALGORITHM

                                    Specify the algorithm to utilize for hashing.

                                    Options:        md5, sha1, sha256, sha384, sha512
                                */

                                case "--algorithm":
                                case "--algo":
                                case "-a":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled && !arg_Benchmark_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/], [#Yellow]--verify[/], or [#Yellow]--benchmark[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Algo_Enabled        = true;

                                    for ( i = i + 1; i < args.Length ; i++ )
                                    {
                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Algo            = property.ToLower( );

                                        /*
                                            CASE > ALGORITHM > HELP
                                        */

                                        if ( arg_Algo == "?" )
                                        {
                                            nl( );
                                            c1(  " A list of algorithms that can be utilized to generate and verify hashes." );
                                            nl( 2 );

                                            c1(  ConsoleColor.Yellow, "         Available Algorithms: " );
                                            nl( );
                                            c1(  ConsoleColor.Gray, "             ", sf( "{0}", arg_Algo_List ) );
                                            nl( );
                                            c1(  ConsoleColor.Gray, "             ", sf( "--algo {0}", arg_Algo_Default ) );
                                            nl( );

                                            return (int)ExitCode.Success;
                                        }
                                    }

                                    if ( arg_Algo_Enabled && ( String.IsNullOrEmpty( arg_Algo ) || arg_Algo.Length < 2 ) )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Specified [#Yellow]--algorithm[/] must be at least two characters, or drop [#Yellow]--algorithm[/] from your command to use the default [#Yellow]" + arg_Algo_Default + "[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    /*
                                        Confirm algorithm
                                    */

                                    var dict_GetHash = Dict_Hashes_Populate( );  // create dictionary for hash algos

                                    if ( !dict_GetHash.Any( x => x.Key.Contains( arg_Algo ) ) )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Unrecognized [#Yellow]--algorithn[/] specified\n" ) );
                                        c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --generate --target <FILE|FOLDER|STRING> --algo sha256[/]\n" ) );
                                        c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --benchmark --algo sha256[/]\n" ) );
                                        c2( sf( " {0,-26} {1,30}", "[#DarkGreen]Options", arg_Algo_List + "[/]\n" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }


                                    break;

                                /*
                                    CASE > LOWERCASE

                                    hash converted to lowercase
                                */

                                case "--lowercase":
                                case "-l":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/] or [#Yellow]--verify[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_LC_Enabled = true;
                                    break;

                                /*
                                    CASE > OVERWRITE

                                    If --output || -o specified
                                    Overwrites the output file instead of appending
                                */

                                case "--overwrite":
                                case "-w":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/] or [#Yellow]--verify[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Overwrite_Enabled = true;
                                    break;

                                /*
                                    CASE > CLIPBOARD

                                    copy generated output to clipboard
                                */

                                case "--clipboard":
                                case "--clip":
                                case "-c":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/] or [#Yellow]--verify[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Clipboard_Enabled = true;
                                    break;

                                /*
                                    CASE > OUTPUT FILE

                                    Specify the output file
                                */

                                case "--output":
                                case "-o":
                                    if ( !arg_VerifyMode_Enabled && !arg_GenMode_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--generate[/] or [#Yellow]--verify[/] arguments[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Output_Enabled      = true;

                                    for ( ++i ; i < args.Length ; i++ )
                                    {
                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        arg_Output_File     = property;
                                    }

                                    if ( arg_Output_Enabled && String.IsNullOrEmpty( arg_Output_File ) )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--output specified but missing [#Yellow]--output <FILE>[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    break;

                                /*
                                    CASE > BENCHMARK

                                    benchmark
                                */

                                case "--benchmark":
                                case "-b":
                                    arg_Benchmark_Enabled   = true;

                                    for ( ++i ; i < args.Length ; i++ )
                                    {
                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        if ( Helpers.bIsNumber( property ) )
                                        {
                                            nl( );
                                            c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Value [#Yellow]" + property.ToString( ) + "[/] is not a valid algorithm [#Yellow]name[/]" ) );
                                            nl( );

                                            return (int)ExitCode.ErrorMissingArg;
                                        }

                                        arg_Algo_Enabled    = true;
                                        arg_Algo            = property;

                                        if ( arg_Algo == "?" )
                                        {
                                            nl( );
                                            c2( sf( " {0,-23} {1,-30}", "[#Green]Help[/]", "  Performs a benchmark on either all algorithms, or a specified algorithm.\n" ) );
                                            c2( sf( " {0,-23} {1,-30}", "[#Green][/]", "  Benchmark has two modes:\n" ) );
                                            c2( sf( " {0,-23} {1,-30}", "[#Green][/]", "     [#Yellow]- Standard Benchmark[/]\n" ) );
                                            c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", "        " + xsum_path_exe + " --benchmark --buffer 32000000 --iterations 50[/]\n" ) );
                                            c2( sf( " {0,-26} {1,-30}", "[#Green][/]", "     [#Yellow]- Algorithm Stress[/]\n" ) );
                                            c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", "        " + xsum_path_exe + " --benchmark --algo sha256 --iterations 100000[/]\n" ) );
                                            nl( );
                                            c2( sf( " {0,-23} {1,-30}", "[#Green][/]", "  [#Yellow]Standard Benchmark[/] will test each algorithm.\n" ) );
                                            c2( sf( " {0,-23} {1,-30}", "[#Green][/]", "  [#Yellow]Algorithm Stress Benchmark[/] will stress the processing speeds of one particular algorithm.\n" ) );
                                            nl( );
                                            c2( sf( " {0,-23} {1,-30}", "[#Green][/]", "  It is highly recommended that you not specify high iteration numbers, or you will\n" ) );
                                            c2( sf( " {0,-23} {1,-30}", "[#Green][/]", "  experience long processing times.\n" ) );
                                            nl( );

                                            return (int)ExitCode.Success;
                                        }

                                    }

                                    break;

                                /*
                                    CASE > ROUNDS

                                    benchmark rounds
                                */

                                case "--iterations":
                                case "-i":
                                    if ( !arg_Benchmark_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--benchmark[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Iterations_Enabled   = true;

                                    for ( ++i ; i < args.Length ; i++ )
                                    {
                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        // not a valid number
                                        if ( !Helpers.bIsNumber( property ) )
                                        {
                                            nl( );
                                            c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Value [#Yellow]" + property.ToString( ) + "[/] is not a valid [#Yellow]number[/]" ) );
                                            nl( );

                                            return (int)ExitCode.ErrorMissingArg;
                                        }

                                        arg_Iterations      = Int32.Parse( property );
                                    }

                                    if ( arg_Benchmark_Enabled && String.IsNullOrEmpty( arg_Iterations.ToString( ) ) )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--iteractions specified but missing [#Yellow]--iterations <NUMBER>[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    // iteractions specified is less than 1
                                    if ( arg_Benchmark_Enabled && arg_Iterations < arg_Iterations_Min )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--iterations value must be at least [#Yellow]" + arg_Iterations_Min.ToString( "N0" ) + "[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    // iterations specified is larger than than 500000
                                    if ( arg_Iterations > arg_Iterations_Max )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--iteractions exceeds [#Yellow]" + arg_Iterations_Max.ToString( "N0" ) + "[/]\n" ) );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red][/]", "This is not recommended, and wont yield different results" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    break;

                                /*
                                    CASE > ROUNDS

                                    benchmark rounds
                                */

                                case "--buffer":
                                    if ( !arg_Benchmark_Enabled )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Must be used with [#Yellow]--benchmark[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    arg_Buffer_Enabled   = true;

                                    for ( ++i ; i < args.Length ; i++ )
                                    {
                                        string property     = args[ i ];
                                        if ( property.StartsWith( "-" ) )
                                        { 
                                            i--;
                                            break;
                                        }

                                        // not a valid number
                                        if ( !Helpers.bIsNumber( property ) )
                                        {
                                            nl( );
                                            c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Value [#Yellow]" + property.ToString( ) + "[/] is not a valid [#Yellow]number[/]" ) );
                                            nl( );

                                            return (int)ExitCode.ErrorMissingArg;
                                        }

                                        arg_Buffer          = Int32.Parse( property );
                                    }

                                    // buffer specified is less than 1
                                    if ( arg_Benchmark_Enabled && arg_Buffer_Enabled && arg_Buffer < arg_Buffer_Min )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--buffer specified but missing [#Yellow]--buffer <NUMBER>[/]" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    // buffer specified is less than 5MB / 5242880 bytes
                                    if ( arg_Buffer < arg_Buffer_Min )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--buffer must be at least [#Yellow]5MB[/] (5242880 bytes)" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    // buffer specified is larger than than 512MB / 512000000 bytes
                                    if ( arg_Buffer > arg_Buffer_Max )
                                    {
                                        nl( );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "--buffer exceeds [#Yellow]512MB[/] (512000000 bytes)\n" ) );
                                        c2( sf( " {0,-23} {1,-30}", "[#Red][/]", "This is not recommended, and wont yield different results" ) );
                                        nl( );

                                        return (int)ExitCode.ErrorMissingArg;
                                    }

                                    break;

                                /*
                                    CASE > DEFAULT
                                */

                                default:
                                    c2( sf( "\n[#Red]Error:     [/]Unknown argument {0}\n", a ) );
                                    return (int)ExitCode.ErrorGeneric;

                                /*
                                    CASE > DEBUG

                                    Enter debug / developer mode.
                                */

                                case "--debug":
                                case "-xdb":
                                    arg_Debug_Enabled = true;
                                    break;

                            }

                        }

                    #endregion

                    if ( inputFile.Count > 0 && arg_Output_File != null )
                    {
               
                    }
                }



                /*
                    Action > Benchmark

                    Triggered when user supplies --benchmark argument.
                    Run benchmark when --benchmark supplied
                */

                #region "Argument: Benchmark"

                    if ( arg_Benchmark_Enabled )
                    {
                        if ( arg_Algo_Enabled )
                        {
                            string algo         = ( arg_Algo_Enabled && !String.IsNullOrEmpty( arg_Algo ) ? arg_Algo : arg_Algo_Default );
                            int iterations      = ( arg_Iterations_Enabled && arg_Iterations != 0  ? arg_Iterations : 100000 );

                            nl( );
                            c0( sf( " {0,-26} {1,-30}", "<#Yellow>Notice:</>", "Running \"<#Yellow>Algorithm Stress</>\" using <#Green>" + algo + "</> and <#Green>" + iterations + "</> iterations" ) );
                            nl( );

                           Benchmark.Perform_Algo_Stress( algo, iterations );
                        }
                        else
                        {
                            int buffer          = ( arg_Buffer_Enabled && arg_Buffer != 0  ? arg_Buffer : 32000000 );
                            int iterations      = ( arg_Iterations_Enabled && arg_Iterations != 0  ? arg_Iterations : 50 );

                            nl( );
                            c0( sf( " {0,-26} {1,-34}", "<#Yellow>Notice:</>", "Running \"<#Yellow>Benchmark</>\" using a buffer of <#Green> " + buffer + "</> and <#Green>" + iterations + "</> iterations" ) );
                            nl( );

                            Benchmark.Start( buffer, iterations );
                        }
                        return (int)ExitCode.Success;
                    }

                #endregion



                /*
                    Action > Sign
                    requires GPG
                */

                #region "Argument: Sign"

                    if ( arg_SignMode_Enabled )
                    {
                        Action_Sign( );
                        return (int)ExitCode.Success;
                    }

                #endregion

                /*
                    Action > Verify

                    Triggered when user supplies --verify argument.
                    Verifies an existing hash digest with the files / folders specified with --target
                */

                #region "Argument: Verify"

                    if ( arg_VerifyMode_Enabled )
                    {
                        Action_Verify( );
                        return (int)ExitCode.Success;
                    }

                #endregion


                /*
                    Action > Generate

                    Triggered when user supplies --generate argument.
                    Generates a new hash digest file from the supplied --target path
                */



                #region "Argument: Generate"

                    if ( arg_GenMode_Enabled )
                    {
                        Action_Generate( );
                        return (int)ExitCode.Success;
                    }

                #endregion

                /*
                    User specified no args.
                    This should not be seen very often if the checks in case switch above are doing their job.
                */

                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Bad [#Yellow]command[/] specified\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --verify[/]\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --generate[/]" ) );
                nl( );

                return (int)ExitCode.ErrorGeneric;
            }

        #endregion

        /*
            Methods to populate and compile Ignored Files dictionary.
        */

        #region "Dictionary: Ignored Files"

            /*
                Ignored Files > Populate Dictionary
                
                some files are automatically added by default by the app, but the user is able to supply their own
                ignored files using --ignore <FILE>
            */

            static IDictionary<string, bool> Dict_Ignored_Populate( bool bDebug = false )
            {

                var dict_Ignored        = new Dictionary<string, bool>( );

                /*
                    String Array >  Ignore List
                    Internal list of files to ignore based on common items usually found in app folders, as well as XSum hash digets.
                */

                string[ ] arr_FilesIgnore = new string[ ]
                {
                    "SHA256.txt",
                    "SHA256.sig",
                    ".gitignore"
                };

                /*
                    Dictionary > Populate Ignored Files
                */

                for ( int i = 0; i < arr_FilesIgnore.Length; i++ )
                {
                    string ignore_file  = arr_FilesIgnore[ i ];
                    dict_Ignored.Add    ( ignore_file, true );

                    if ( AppInfo.bIsDebug( ) || bDebug )
                        wl( sf( " + Ignoring {0}", ignore_file ) );
                }

                return dict_Ignored;
            }

            /*
                Ignored Files > Get List

                This reports back to the user what files have been ignored with the current task.
            */

            static string Ignored_GetList( )
            {
                string str_lst          = "";
                var dict_Ignored        = Dict_Ignored_Populate( );

                StringBuilder sb        = new StringBuilder( );

                int i_cur               = 0;
                int i_max               = dict_Ignored.Keys.Count;

                foreach ( string file in dict_Ignored.Keys )
                {
                    i_cur++;
                    sb.Append( ( i_cur == i_max ) ? file : file + ", " );
                    str_lst = sb.ToString( );
                }

                return str_lst;
            }

        #endregion

        /*
            Write Output

            Saves results to a file.
            Requires --output argument.

            @arg        : str file
            @arg        : str algo
            @arg        : sb output
            @arg        : bool bOverwrite
        */

        static async Task WriteOutput( string file, string algo, StringBuilder output, bool bOverwrite )
        {
            await Task.Run( ( ) =>
            {
                string file_saveto = null;

                if ( IsFullPath( file ) )
                {
                    if ( Path.HasExtension( file ) )
                        file_saveto         = file;
                    else
                        file_saveto         = sf( @"{0}\{1}.txt", file, algo );
                }
                else
                {
                    if ( Path.HasExtension( file ) )
                        file_saveto         = sf( @"{0}\{1}", xsum_path_dir, file );
                    else
                        file_saveto         = sf( @"{0}\{1}\{2}.txt", xsum_path_dir, file, algo );
                }

                /*
                    output > save to file
                */

                if ( !String.IsNullOrEmpty( file_saveto ) )
                {
                    output.Append( Environment.NewLine );
                    output.Append( Environment.NewLine );

                    if ( arg_Progress_Enabled )
                    {
                        nl( );
                        c2( String.Format( " {0,-25} {1,-30}", "[#DarkGray]Save:", String.Format( file_saveto ) + "[/]" ) );
                        nl( );
                    }

                    using ( StreamWriter writer = new StreamWriter( file_saveto, !bOverwrite ) )
                    {
                        writer.WriteLine( output.ToString() );
                    }
                }
            } );
        }

        /*
            Generate

            This method does a few things.
            Accepts a --target <STRING|FOLDER|FILE>

            If the user specifics a file or folder, the hash for that will be displayed.

            If the target provided cannot be found on the user's system as a valid file or folder, it will
            enter "Sring Mode", and output the hash of the string they provided.
        */

        static public int Action_Generate( )
        {
            var dict_GetHash            = Dict_Hashes_Populate( );                          // create dictionary for hash algos
            var dict_Ignored            = Dict_Ignored_Populate( arg_Debug_Enabled );       // create dictionary for ignored files            

            /*
                Define > StringBuilder output
                utilized to --output results
            */

            StringBuilder sb_output     = new StringBuilder( );

            /*
                Output > create header for top of --output file
            */

            DateTime dt             = DateTime.Now;
            string now              = dt.ToString( "MMMM dd yyyy" );
            string h1               = sf( " ------------------------------------------------------------------------------------------\n" );
            string h2               = sf( " {0,-20} {1,30}", now, "---------------------------------------------------------------------\n" );
            string h3               = sf( " ------------------------------------------------------------------------------------------\n" );

            sb_output.Append( h1 );
            sb_output.Append( h2 );
            sb_output.Append( h3 );

            /*
               User did not specify --target
            */

            if ( String.IsNullOrEmpty( arg_Target_File ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "No [#Yellow]--target[/] file, folder, or string specified\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --generate --target <FILE|FOLDER|STRING>[/]\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --verify --target <FILE|FOLDER>[/]" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            /*
                User specified invalid --algo not in the list
            */

            if ( !dict_GetHash.Any( x => x.Key.Contains( arg_Algo ) ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Unrecognized [#Yellow]--algorithn[/] specified\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --generate --target <FILE|FOLDER|STRING>[/]\n" ) );
                c2( sf( " {0,-26} {1,30}", "[#DarkGreen]Options", arg_Algo_List + "[/]\n" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            /*
                --verify <FILE> provided, check if it exists as a FILE or FOLDER
            */

            bool targ_bIsFile       = false;
            bool targ_bIsFolder     = false;
            bool targ_bIsString     = false;
            string targ_hash        = null;

            if ( !String.IsNullOrEmpty( arg_Target_File ) )
            {
                /*
                    --verify "file.txt"
                */

                if ( File.Exists( arg_Target_File ) )
                    targ_bIsFile    = true;

                /*
                    --verify "Test\"
                */

                if ( Directory.Exists( arg_Target_File ) )
                    targ_bIsFolder  = true;
            }

            if ( AppInfo.bIsDebug( ) || arg_Debug_Enabled )
            {
                nl( );
                wl( sf( " targ_bIsFile ......... {0}", targ_bIsFile ) );
                wl( sf( " targ_bIsFolder ....... {0}", targ_bIsFolder ) );
                nl( );
            }

            /*
                Digest File

                User specified --digest but did not supply a location to save the digest.
                Automatically generate a path to save the digest file

                @default    :       [ALGO].txt
                                    SHA256.txt
            */

            if ( String.IsNullOrEmpty( arg_Digest_File ) )
            {
                arg_Digest_File = String.Format( "{0}.txt", arg_Algo.ToUpper( ) );
            }

            /*
                Target is Folder
                
                Get full path of target and then get the hash of the folder

                @output         : X:\Path\To\Folder
            */

            if ( targ_bIsFolder )
            {
                targ_hash               = Path.GetFullPath( arg_Target_File );
                targ_hash               = Hash.GetHash_Directory( arg_Algo,  targ_hash );
                targ_hash               = ( arg_LC_Enabled ? targ_hash.ToLower( ) : targ_hash );
            }
 
            /*
                Target is File

                Combine target path to generate a full path and then grab the hash.
                Path.Combine will automatically ignore adding the first argument if it detects that
                the second path is absolute.

                @output         : X:\Path\To\Folder\file.xxx
            */

            if ( targ_bIsFile )
            {
                targ_hash               = Path.Combine( xsum_path_dir, arg_Target_File );
                targ_hash               = dict_GetHash[ arg_Algo ]( targ_hash );
                targ_hash               = ( arg_LC_Enabled ? targ_hash.ToLower( ) : targ_hash );
            }

            /*
                Target is String
            
                Return the hash of a string specified as the target.
                This gets triggered if the system cannot detect a valid file or folder provided.
            */

            if ( String.IsNullOrEmpty( targ_hash ) )
            {
                targ_bIsString          = true;
                targ_hash               = Hash.GetHash_String( arg_Algo, arg_Target_File );
                targ_hash               = ( arg_LC_Enabled ? targ_hash.ToLower( ) : targ_hash );
            }

            /*
                current paths

                @output         : X:\Path\To\Folder
            */

            string file_path_search     = Path.GetFullPath( arg_Target_File );

            /*
                targ_bIsFile            bool means the user is trying to verify a single file instead of a folder
            */

            if ( targ_bIsFile )
            {
                file_path_search        = Path.GetDirectoryName( Path.Combine( xsum_path_dir, arg_Target_File ) );      // H:\CSharpProjects\XSum\bin\Release\net481
                file_path_search        = Path.Combine( file_path_search, arg_Target_File );
            }

            /*
                Verify search path exists
            */

            if ( targ_bIsFolder && !Directory.Exists( file_path_search ) || targ_bIsFile && !File.Exists( file_path_search ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Could not locate [#Yellow]" + file_path_search + "[/]" ) );
                nl( );

                return (int)ExitCode.ErrorGeneric;
            }

            /*
                loop files found in directory of search path
            */

            var dict_digest             = new Dictionary<string, string>( );
            StringBuilder sb_digest     = new StringBuilder( );

            /*
                Mode: Folder
            */

            if ( targ_bIsFolder )
            {

                /*
                    get all files and folders that exist in file_path_search
                    this path should only be a folder.

                    if user specifics a file, find the folder it exists in and use that directory
                */

                string[] files          = Directory.GetFiles( file_path_search, "*", SearchOption.AllDirectories );

                if ( files.Length < 1 )
                {
                    nl( );
                    c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Could not locate sub path from [#Yellow]" + file_path_search + "[/]" ) );
                    nl( );

                    return (int)ExitCode.ErrorGeneric;
                }

                for ( int i = 0; i < files.Length; i++ )
                {
                    var ( hash, name ) = NewHash_File( files[ i ] );
                    if ( hash == null || name == null )
                        return (int)ExitCode.ErrorGeneric;

                    dict_digest.Add( hash, name );
                }
            }

            /*
                Mode: File
            */

            else if ( targ_bIsFile )
            {
                var ( hash, name ) = NewHash_File( file_path_search );
                if ( hash == null || name == null )
                    return (int)ExitCode.ErrorGeneric;

                dict_digest.Add( hash, name );
            }

            /*
                Mode: String
            */

            else
            {
                dict_digest.Add( targ_hash, arg_Target_File );
            }

            /*
                Output Category

                Figure out what type of target was provided to display to the user.
                    - targ_bIsFile      = File
                    - targ_bIsFolder    = Folder
                    - targ_bIsString    = String
            */

            string item_type    = targ_bIsFile ? "file" : targ_bIsFolder ? "folder" : targ_bIsString ? "string" : "unknown";
            string OW_Status    = ( arg_Overwrite_Enabled ? "Enabled" : "Disabled" );
            string LC_Status    = ( arg_LC_Enabled ? "Enabled" : "Disabled" );
            string CB_Status    = ( arg_Clipboard_Enabled ? "Enabled" : "Disabled" );

            /*
                --output log
            */

            sb_output.Append        ( Environment.NewLine );
            string s1               = sf( " Generated hash for {0} \"{1}\" using \"{2}\"", item_type, arg_Target_File, arg_Algo.ToUpper( ) );
            sb_output.Append        ( s1 );
            sb_output.Append        ( Environment.NewLine );

            sb_output.Append        ( Environment.NewLine );
            string s2               = sf( " {0,-20} {1,-30}", "Overwrite:", "" + OW_Status + "" );
            sb_output.Append        ( s2 );

            sb_output.Append        ( Environment.NewLine );
            string s3               = sf( " {0,-20} {1,-30}", "Lowercase:", "" + LC_Status + "" );
            sb_output.Append        ( s3 );

            sb_output.Append        ( Environment.NewLine );
            string s4               = sf( " {0,-20} {1,-30}", "Clipboard:", "" + CB_Status + "" );
            sb_output.Append        ( s4 );
            sb_output.Append        ( Environment.NewLine );

            /*
                write hashes and filenames to digest file
            */

            using ( StreamWriter file = new StreamWriter( arg_Digest_File ) )
            {

                if ( arg_Progress_Enabled )
                    nl( );

                foreach ( var entry in dict_digest )
                {
                    sb_output.Append        ( Environment.NewLine );
                    string s5               = sf( " {0}  {1}", entry.Key, entry.Value );
                    sb_output.Append        ( s5 );

                    // only shown if user uses --progress
                    if ( arg_Progress_Enabled )
                    {
                        string l1   = sf( " {0,-15}{1,-30}", "Success", entry.Key );
                        string l2   = sf( " {0,-15}{1,-30}", "", entry.Value );

                        wc( "Green" );
                        wl( l1 );
                        wc( "DarkGray" );
                        wl( l2 );
                        nl( );
                    }

                    file.WriteLine( "{0}  {1}", entry.Key, entry.Value ); 
                }
            }

            if ( arg_Progress_Enabled )
            {

                /*
                    Extras > Overwrite
                */

                c2( sf( " {0,-28} {1,-30}", "[#DarkGray]Overwrite:[/]", "[#DarkGray]" + OW_Status + "[/]" ) );
                nl( );

                /*
                    Extras > Lowercase
                */

                c2( sf( " {0,-28} {1,-30}", "[#DarkGray]Lowercase:[/]", "[#DarkGray]" + LC_Status + "[/]" ) );
                nl( );

                /*
                    Extras > Clipboard
                */

                c2( sf( " {0,-28} {1,-30}", "[#DarkGray]Clipboard:[/]", "[#DarkGray]" + CB_Status + "[/]" ) );
                nl( );

            }

            /*
                Extras > Clipboard > Copy
            */

            if ( arg_Clipboard_Enabled )
                Helpers.SetClipboard( targ_hash );

            /*
                Output to user
            */

            nl( );
            c2( String.Format( new InitSF( ), " {0,-24} {1,-30}", "[#Blue]" + Helpers.ucfirst( item_type ) + " Mode:[/]", "Generated hash for " + item_type + " [#Yellow]\"" + arg_Target_File + "\"[/] using [#Yellow]" + arg_Algo.ToUpper( ) + "[/]" ) );
            nl( ); 
            c2( String.Format( new InitSF( ), " {0,-24} {1,-30}", "[#Blue][/]", "Digest saved to [#Yellow]\"" + arg_Digest_File + "\"[/]" ) );
            nl( );
            nl( );

            /*
                Output > Hash
            */

            c2( sf( " {0,-14} {1,-20}", " ", targ_hash ) );
            nl( );

            /*
                Argument --output
                Results will be stored in a file
            */

            if ( !String.IsNullOrEmpty( arg_Output_File ) )
            {

                /*
                    Determine save location

                    if provided output only contains a local folder with no filename:
                        -   x:\Path\To\XSum\folder\{algo}.txt

                    If provided output contains a local folder with filename:
                        -   x:\Path\To\XSum\folder\filename.xxx

                    If provided output is a full path without a filename:
                        -   x:\path\provided\{algo}.txt

                    If provided output hs a full path with filename:
                        -   x:\path\provided\filename.xxx
                */

                WriteOutput( arg_Output_File, arg_Algo, sb_output, arg_Overwrite_Enabled ).Wait( );

            }

            return (int)ExitCode.Success;
        }

        /*
            Generate Hash > File

            @arg        : str name
        */

        static public( string hash, string name ) NewHash_File( string filename )
        {

            if ( !File.Exists( filename ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Could not locate file [#Yellow]" + filename + "[/]" ) );
                nl( );
                
                return ( null, null );
            }

            /*
                IF USER SPECIFYIES SINGLE FILE:         Script will find out what folder that file is in.
                                                        Then it will scan that entire folder and display ALL files
                                                        that exist next to that specific file.

                IF USER SPECIFIES FOLDER:               Every file that exists in that folder and subfolders will
                                                        be cycled through.
            */

            string item_file_name       = Path.GetFileName( filename );

            /*
                If user specifies a file only, get the full path to that file.

                Path.Combine:           If the one of the subsequent paths is an absolute path, then the combine operation 
                                        resets starting with that absolute path, discarding all previous combined paths.
            */

            string item_path_full       = Path.Combine( xsum_path_dir, filename );

            /*
                get the relative path to the file that will be inserted into the digest
            */

            Uri item_relative_file      = new Uri( @item_path_full );
            Uri item_relative_folder    = new Uri( @xsum_path_dir + @"\" );             // must end in backslash

            string item_path_relative   = Uri.UnescapeDataString( item_relative_folder.MakeRelativeUri( item_relative_file ).ToString( ).Replace( '/', Path.DirectorySeparatorChar ) );
            item_path_relative          = item_path_relative.Replace( @"\", "/" );      // replace backslash with forwardslash to make lines compatible with unix

            /*
                get file hash
            */

            StringBuilder sb_digest     = new StringBuilder( );
            var dict_digest             = new Dictionary<string, string>( );
            var dict_GetHash            = Dict_Hashes_Populate( );                                                      // create dictionary for hash algos
            var dict_Ignored            = Dict_Ignored_Populate( arg_Debug_Enabled );                                   // create dictionary for ignored files

            string item_get_hash        = dict_GetHash[ arg_Algo ]( item_path_full );                                   // get hash of file
            item_get_hash               = ( arg_LC_Enabled ? item_get_hash.ToLower( ) : item_get_hash );                // hash to upper or lower
            var item_bDoIgnore          = dict_Ignored.FirstOrDefault ( x=> x.Key.Contains( item_file_name ) );         // check if file is ignored

            // files in ignore list
            if ( item_bDoIgnore.Value )
                return ( null, null );

            /*
                ignore empty files
            */

            if ( new FileInfo( item_path_full ).Length == 0 )
                return ( null, null );

            return ( item_get_hash, item_path_relative );
        }


        /*
            ACTION > SIGN
        */

        static public int Action_Sign( )
        {

            /*
                make sure gpg is installed and the path is added to your windows environment variables
            */

            bool bFind_GPG = FindProgram( "gpg.exe" );

            if ( !bFind_GPG )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Cannot sign digests without [#Yellow]gpg.exe[/]" ) );
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red][/]", "You must install [#Yellow]GPG[/] and add it to your Windows[/]" ) );
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red][/]", "environment variables before you can use this feature.[/]" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            return (int)ExitCode.ErrorGeneric;
        }

        /*
            ACTION > VERIFY

            Verifies a hash digest.
            Triggered when user specifies --verify
        */

        static public int Action_Verify( )
        {

            var dict_GetHash            = Dict_Hashes_Populate( );                          // create dictionary for hash algos
            var dict_Ignored            = Dict_Ignored_Populate( arg_Debug_Enabled );       // create dictionary for ignored files

            /*
                Define > StringBuilder output
                utilized to --output results
            */

            StringBuilder sb_output     = new StringBuilder( );

            if ( !dict_GetHash.Any( x => x.Key.Contains( arg_Algo ) ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Unrecognized [#Yellow]--algorithm[/]\n" ) );
                c2( sf( " {0,-26} {1,-30}", "[#DarkGreen]Options", arg_Algo_List + "[/]\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --verify --algorithm " + arg_Algo_Default + "[/]" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            /*
                VERIFY > NO VERIFICATION FILE PROVIDED
            */

            if ( String.IsNullOrEmpty( arg_Target_File ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Missing [#Yellow]--target[/]\n" ) );
                c2( sf( " {0,-24} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --verify --target <FILE|FOLDER>[/]\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --generate --target <FILE|FOLDER|STRING>[/]" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            /*
                --verify <FILE> provided, check if it exists as a FILE or FOLDER
            */

            bool targ_bIsFile       = false;
            bool targ_bIsFolder     = false;

            if ( !String.IsNullOrEmpty( arg_Target_File ) )
            {
                /*
                    --verify "file.pdb"
                */

                if ( File.Exists( arg_Target_File ) )
                    targ_bIsFile    = true;

                /*
                    --verify "Test"
                */

                if ( Directory.Exists( arg_Target_File ) )
                    targ_bIsFolder  = true;
            }

            if ( AppInfo.bIsDebug( ) || arg_Debug_Enabled )
            {
                nl( );
                wl( sf( " targ_bIsFile ......... {0}", targ_bIsFile ) );
                wl( sf( " targ_bIsFolder ....... {0}", targ_bIsFolder ) );
                nl( );
            }

            /*
                user failed to specify a file or folder to check via 
                    --verify <FILE or FOLDER>
            */

            if ( !targ_bIsFile && !targ_bIsFolder )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Specified target file or folder [#Yellow]\"" + arg_Target_File + "\"[/] does not exist[/]\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --verify --target <FILE|FOLDER>[/]" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            /*
                user specified --verify, but missing --digest <FILE>
            */

            if ( ( !arg_Digest_Enabled) || ( !arg_Digest_Enabled && String.IsNullOrEmpty( arg_Digest_File ) ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Missing [#Yellow]--digest[/]\n" ) );
                c2( sf( " {0,-24} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --verify --target <FILE|FOLDER> --digest <HASH_DIGEST_FILE>[/]" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            /*
                user specified --digest but digest file doesnt exist
            */

            if ( arg_Digest_Enabled && !File.Exists( arg_Digest_File ) )
            {
                nl( );
                c2( sf( " {0,-23} {1,-30}", "[#Red]Error[/]", "Specified [#Yellow]--digest \"" + arg_Digest_File + "\"[/] file does not exist\n" ) );
                c2( sf( " {0,-25} {1,-30}", "[#DarkGray] ", xsum_path_exe + " --verify --target <FILE|FOLDER> --digest <HASH_DIGEST_FILE>[/]" ) );
                nl( );

                return (int)ExitCode.ErrorMissingArg;
            }

            /*
                current paths

                @output         : X:\Path\To\Folder
            */

            string file_path_search     = Path.GetFullPath( arg_Target_File );

            /*
                targ_bIsFile        bool means the user is trying to verify a single file instead of a folder
            */

            if ( targ_bIsFile )
                file_path_search        = Path.GetDirectoryName( Path.Combine( xsum_path_dir, arg_Target_File ) );      // H:\CSharpProjects\XSum\bin\Release\net481

            /*
                open digest file and grab hash / filename of each entry

                dont remember why, but there was a purpose to splutting space a single time on char and then trimming string.
            */

            string row_hash             = null;
            string row_name             = null;
            var digest_lines            = File.ReadAllLines( arg_Digest_File );                 // read hash digest file ( SHA256.sig or SHA256.txt )
            var dict_digest             = new Dictionary<string, string>( );                    // create dictionary to store name and name in

            for ( var i = 0; i < digest_lines.Length; i += 1 )
            {
                var digest_line         = digest_lines[ i ];                                    // define each row
                var digest_split        = digest_line.Split( new[] { ' ' }, 2 );                // split file and hash at space

                // split didt produce 2 objects
                if ( ( digest_split.Length < 2 ) )
                    continue;

                row_hash                = digest_split[ 0 ].Trim( );                            // b3d7za...
                row_name                = digest_split[ 1 ].Trim( );                            // X:\Path\To\File

                row_hash                = ( arg_LC_Enabled ? row_hash.ToLower( ) : row_hash );  // hash of file ( lowercase or normal )
                row_name                = row_name;                                             // name of file

                if ( !dict_digest.ContainsKey( row_hash ) )
                    dict_digest.Add     ( row_hash, row_name );
            }

            /*
                create header for top of --output file
            */

            DateTime dt             = DateTime.Now;
            string now              = dt.ToString( "MMMM dd yyyy" );
            string h1               = sf( " ------------------------------------------------------------------------------------------\n" );
            string h2               = sf( " {0,-20} {1,30}", now, "---------------------------------------------------------------------\n" );
            string h3               = sf( " ------------------------------------------------------------------------------------------\n" );

            sb_output.Append( h1 );
            sb_output.Append( h2 );
            sb_output.Append( h3 );

            /*
                get all files and folders that exist in file_path_search
                this path should only be a folder.

                if user specifics a file, find the folder it exists in and use that directory
            */

            string[] files          = Directory.GetFiles( file_path_search, "*", SearchOption.AllDirectories );     // get all folders

            int i_total             = files.Length;                                             // total files in directories
            int i_count             = 0;                                                        // total files scanned
            int i_success           = 0;                                                        // total files correct hash
            int i_error             = 0;                                                        // total files mismatch hash

            string item_hash        = ( targ_bIsFolder ? Hash.GetHash_Directory( arg_Algo,  arg_Target_File ) : null );
            item_hash               = (  arg_LC_Enabled ? item_hash.ToLower( ) : item_hash );

            string item_Target      = null;
            bool bFailed            = false;
            bool bSingleFile        = false;

            /*
                loop through all the files inside the directory searched above
            */

            nl( );

            for ( int i = 0; i < files.Length; i++ )
            {
                // previous file failed match, stop
                if ( !arg_Progress_Enabled && bFailed ) break;
                if ( bSingleFile ) break;

                // next file in folder
                // full path to each file in directory / sub dir
                string item_path_full       = files[ i ];

                /*
                    IF USER SPECIFYIES SINGLE FILE:         Script will find out what folder that file is in.
                                                            Then it will scan that entire folder and display ALL files
                                                            that exist next to that specific file.

                    IF USER SPECIFIES FOLDER:               Every file that exists in that folder and subfolders will
                                                            be cycled through.
                */

                string item_file_name       = Path.GetFileName( item_path_full );

                /*
                    If user specifies a file only, get the full path to that file.

                    Path.Combine:           If the one of the subsequent paths is an absolute path, then the combine operation 
                                            resets starting with that absolute path, discarding all previous combined paths.
                */

                if ( targ_bIsFile )
                    item_path_full          = Path.Combine( xsum_path_dir, arg_Target_File );
 
                /*
                    hash for specified file  dict_GetHash[ "sha256" ]( "X:\Full\Path\To\File.zip" )
                */

                string item_get_hash        = dict_GetHash[ arg_Algo ]( item_path_full ); 
                item_get_hash               = ( arg_LC_Enabled ? item_get_hash.ToLower( ) : item_get_hash ); // hash to upper or lower
                var item_FoundMatch         = dict_digest.FirstOrDefault    ( x=> x.Key.Contains( item_get_hash ) );    // find the matching hash
                var item_bDoIgnore          = dict_Ignored.FirstOrDefault   ( x=> x.Key.Contains( item_file_name ) );   // check if file is ignored

                // files in ignore list
                if ( item_bDoIgnore.Value ) continue;

                // found matching file in dictionary
                if ( !String.IsNullOrEmpty( item_FoundMatch.Key ) )
                {
                    item_Target     = item_path_full;

                    string l1   = sf( " {0,-15}{1,-30}", "Success", item_path_full );
                    string l2   = sf( " {0,-15}{1,-30}", "", item_get_hash );

                    // only shown if user uses --progress
                    if ( arg_Progress_Enabled )
                    {
                        wc( "Green" );
                        wl( l1 );
                        wc( "DarkGray" );
                        wl( l2 );
                        nl( );
                    }

                    sb_output.Append( Environment.NewLine );
                    sb_output.Append( l1 );
                    sb_output.Append( Environment.NewLine );
                    sb_output.Append( l2 );
                    sb_output.Append( Environment.NewLine );

                    /*
                        if user looks for a file instead of folder, we only want the one file displaying
                            --verify "X:\Full\Path\To\File.zip"

                        without this, xsum will continue to loop through the rest of the files
                    */

                    if ( targ_bIsFile )
                        bSingleFile = true;

                    i_success++;
                }
                else
                {
                    item_Target     = item_path_full;

                    string l5       = sf( " {0,-15}{1,-30}", "Fail", item_path_full );
                    string l6       = sf( " {0,-15}{1,-30}", "", item_get_hash );

                    // only shown if user uses --progress
                    if ( arg_Progress_Enabled )
                    {
                        wc( "Red" );
                        wl( l5 );
                        wc( "DarkGray" );
                        wl( l6 );
                        nl( );
                    }

                    sb_output.Append( Environment.NewLine );
                    sb_output.Append( l5 );
                    sb_output.Append( Environment.NewLine );
                    sb_output.Append( l6 );
                    sb_output.Append( Environment.NewLine );

                    /*
                        if user looks for a file instead of folder, we only want the one file displaying
                            --verify "Path\To\File.xxx"
                    */

                    if ( targ_bIsFile )
                        bSingleFile = true;

                    bFailed         = true;
                    i_error++;
                }

                i_count++;
            }

            /*
                Extra arguments
            */

            string LC_Status        = ( arg_LC_Enabled ? "Enabled" : "Disabled" );
            c2( sf( " {0,-28} {1,-30}", "[#DarkGray]Lowercase:[/]", "[#DarkGray]" + LC_Status + "[/]" ) );
            nl( );

            /*
                Clipboard
            */

            string CB_Status        = ( arg_Clipboard_Enabled ? "Enabled" : "Disabled" );
            c2( sf( " {0,-28} {1,-30}", "[#DarkGray]Clipboard:[/]", "[#DarkGray]" + CB_Status + "[/]" ) );
            nl( );

            if ( arg_Clipboard_Enabled && targ_bIsFolder )
                Helpers.SetClipboard( item_hash );

            /*
                Prepare output
            */

            nl( );

            FileAttributes attr     = File.GetAttributes( @arg_Target_File );
            string verify_file_type = attr.HasFlag( FileAttributes.Directory ) ? "Folder" : "File";

            sb_output.Append        ( Environment.NewLine );
            c2( sf( " [#Yellow]Using \"{0}\" to verify hash of {1} \"{2}\"[/]", arg_Algo, verify_file_type, arg_Target_File ) );

            string s0               = sf( " Using \"{0}\" to verify hash of {1} \"{2}\"", arg_Algo, verify_file_type, arg_Target_File );
            sb_output.Append        ( s0 );

            nl( );

            /*
                Success
            */

            if ( !bFailed )
            {
                sb_output.Append    ( Environment.NewLine );
                c2( sf( " [#Green]Successfully verified {0} [#Gray]{1}[#Green] in digest [#Gray]{2}[/]", verify_file_type, arg_Target_File, arg_Digest_File ) );

                nl( );

                string s1           = sf( " Successfully verified {0} \"{1}\" in hash digest \"{2}\"", verify_file_type, arg_Target_File, arg_Digest_File );
                sb_output.Append    ( s1 );
                sb_output.Append    ( Environment.NewLine );

                rs( );
            }

            /*
                Fail

                When fail occurs, list the actual file that failed
                    - verify_file_type          Name of file or folder user wants to check
                    - item_Target               Name of the file that failed when targeting folder
            */

            else
            {
                FileAttributes targ_attr    = File.GetAttributes( @item_Target );
                string target_file_type     = targ_attr.HasFlag( FileAttributes.Directory ) ? "Folder" : "File";

                sb_output.Append    ( Environment.NewLine );
                c2( sf( " [#Red]{0} [#Gray]{1}[#Red] not found in hash digest [#Gray]{2}[/]\n", target_file_type, item_Target, arg_Digest_File ) );

                string s1           = String.Format( " {0} {1} not found in hash digest {2}", target_file_type, item_Target, arg_Digest_File );
                sb_output.Append    ( s1 );
                sb_output.Append    ( Environment.NewLine );

                rs( );
            }

            /*
                display additional info if --progress supplied
            */

            if ( arg_Progress_Enabled )
            {

                nl( );
                sb_output.Append( Environment.NewLine );

                string s2 = String.Format( " {0,-15}{1,-30}", i_count, String.Format( "files scanned" ) );
                wc( "Gray" );
                wl( s2 );
                sb_output.Append( s2 );
                sb_output.Append( Environment.NewLine );

                string s3 = String.Format( " {0,-15}{1,-30}", i_success, String.Format( "files successfully verified" ) );
                wc( "Green" );
                wl( s3 );
                sb_output.Append( s3 );
                sb_output.Append( Environment.NewLine );

                string s4 = String.Format( " {0,-15}{1,-30}", i_error, String.Format( "files failed verification" ) );
                wc( "Red" );
                wl( s4 );
                sb_output.Append( s4 );
                sb_output.Append( Environment.NewLine );

                if ( i_count == i_success )
                {
                    fg( ConsoleColor.DarkGreen );
                    string s5 = String.Format( "           All files successfully verified" );
                    wl( s5 );

                    sb_output.Append( s5 );
                    rs( );
                }
            }

            /*
                Argument --output
                Results will be stored in a file
            */

            if ( !String.IsNullOrEmpty( arg_Output_File ) )
            {

                /*
                    Determine save location

                    if provided output only contains a local folder with no filename:
                        -   x:\Path\To\XSum\folder\{algo}.txt

                    If provided output contains a local folder with filename:
                        -   x:\Path\To\XSum\folder\filename.xxx

                    If provided output is a full path without a filename:
                        -   x:\path\provided\{algo}.txt

                    If provided output hs a full path with filename:
                        -   x:\path\provided\filename.xxx
                */

                WriteOutput( arg_Output_File, arg_Algo, sb_output, arg_Overwrite_Enabled ).Wait( );

            }

            return (int)ExitCode.Success;

        }

    }

}
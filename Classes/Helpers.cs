
#region "Using"

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections;
using System.IO;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.ComponentModel;
using System.Linq;

#endregion

namespace XSum
{

    /*
        Class > Helpers
    */

    public class Helpers
    {

        /*
            Define > File Name
            utilized with Log module.
        */

        #region "Define: Fileinfo"

            readonly static string log_file = "Helpers.cs";

        #endregion



        /*
            Format Wildcard

            @ex         : bWCIgnore = Regex.IsMatch( item_variable, Helpers.WildcardMatch( "*.txt ) );
        */

        static public string WildcardMatch( string str )
        {
            return "^" + Regex.Escape( str ).Replace( "\\?", "." ).Replace( "\\*", ".*" ) + "$"; 
        }

        /*
            String Array to String List

            converts a string[] into a string
        */

        static public string GetStringArray( string[] arr )
        {
            var result = string.Empty;
            foreach ( var item in arr )
            {
                result += item;
            }
            return result;
        }

        /*
            Dictionary to String List

            converts a string[] into a string
        */

        static public string GetStringDictionary( IDictionary dict )
        {
            string str_lst          = "";
            var dict_Ignore         = dict;

            StringBuilder sb        = new StringBuilder( );

            int i_cur               = 0;
            int i_max               = dict_Ignore.Keys.Count;

            foreach ( string file in dict_Ignore.Keys )
            {
                i_cur++;
                sb.Append( ( i_cur == i_max ) ? file : file + ", " );
                str_lst = sb.ToString( );
            }

            return str_lst;

        }

        /*
            Get Path by Wildcard

            @arg        : str path
            @arg        : str wildcard

            @usage      : string[] files = Helpers.GetWildcardFiles( @xsum_path_dir, @wildcard );
                          string[] files = Helpers.GetWildcardFiles( @xsum_path_dir, @"SHA*.txt" );
        */

        static public string[] GetWildcardFiles( string path, string wildcard )
        {
            string dir_root         = path; 
            string search_wildcard  = wildcard;

            string pattern          = Path.GetFileName( search_wildcard ); 
            string relDir           = search_wildcard.Substring ( 0, search_wildcard.Length - pattern.Length );
            string path_absolute    = Path.GetFullPath ( Path.Combine ( dir_root ,relDir ) );

            string[] files          = Directory.GetFiles ( path_absolute, pattern, SearchOption.TopDirectoryOnly );

            return files;
        }

        /*
            Console

            @str        : str msg
            @ret        : void
        */

        static public void c0( string str )
        {
            string[] msg        = str.Split( '<','>' );
            ConsoleColor        clr;

            Console.ForegroundColor = ConsoleColor.White;

            foreach( var resp in msg )
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

        /*
            Set clipboard text

            only works with Windows, if we make this app work with Linux, an alternative will need to be used.

            @arg        : str str
            @ret        : void
        */

        public static void SetClipboard( string str )
        {

            if ( str == null )
                throw new ArgumentNullException( "Attempted to set clipboard to NULL" );

            Process cb          = new Process( ); 
            cb.StartInfo        = new ProcessStartInfo
            {
                RedirectStandardInput   = true,
                FileName                = @"clip",
                UseShellExecute         = false
            };

            cb.Start( );
            cb.StandardInput.Write( str );
            cb.StandardInput.Close( ); 

            return;

        }

        /*
            Converts the first letter of a string to uppercase

            @arg        : str str
            @ret        : str
        */

        static public string ucfirst( string str )
        {
            if ( string.IsNullOrEmpty( str ) )
                return string.Empty;

            return $"{str[ 0 ].ToString( ).ToUpper( )}{str.Substring( 1 ) }";
        }

        /*
            Create Random String

            Used for benchmarking
            generates a large set of words that will be used to benchmark sha algorithms

            @arg        : str str
            @bool       : bool bRandCase
            @ret        : str
        */

        public string RandomString( int num_words, bool bRandCase )
        {
            var sb          = new System.Text.StringBuilder( );
            Random rand     = new Random( ); 

            for ( int a = 0; a < num_words; a++ ) 
            { 

                int rand_val; 
                char letter; 
                int word_len    = rand.Next( 5, 20 ); 
                string word     = "";

                for ( int i = 0; i < word_len; i++ ) 
                {
                    rand_val    = rand.Next( 0, 26 );
                    letter      = Convert.ToChar( rand_val + 65 );

                    if ( i % 2 == 0 && bRandCase )
                    {
                        letter = char.ToLower( letter );
                    }

                    word        += letter;
                } 

                sb.Append( word + " " );
            } 

            return sb.ToString( );
        }


        /*
            Method: Table > Align Text > Left

            @ex         : Helpers.ALL( "String Text", 100  ), typeof(string) );
            @arg        : str text
            @arg        : int width
            @ret        : str
        */

        public static string ALL( string text, int width )
        {
            text = text.Length > width ? text.Substring( 0, width - 3 ) + "..." : text;

            if ( string.IsNullOrEmpty( text ) )
                return new string( ' ', width );
            else
                return text.PadRight( width - ( text.Length ) / 2 ).PadRight( width );
           
        }

        /*
            Method: Table > Align Text > Center

            @ex         : Helpers.ALC( "String Text", 100  ), typeof(string) );
            @arg        : str text
            @arg        : int width
            @ret        : str
        */

        public static string ALC( string text, int width )
        {
            text = text.Length > width ? text.Substring( 0, width - 3 ) + "..." : text;

            if ( string.IsNullOrEmpty( text ) )
                return new string( ' ', width );
            else
                return text.PadRight( width - ( width - text.Length ) / 2 ).PadLeft( width );
           
        }

        /*
            Method: Table > Align Text > Right

            @ex         : Helpers.ALR( "String Text", 100  ), typeof(string) );
            @arg        : str text
            @arg        : int width
            @ret        : str
        */

        public static string ALR( string text, int width )
        {
            text = text.Length > width ? text.Substring( 0, width - 3 ) + "..." : text;

            if ( string.IsNullOrEmpty( text ) )
                return new string( ' ', width );
            else
                return text.PadLeft( width - ( text.Length ) / 2 ).PadLeft( width );
           
        }

        /*
            Check if provided value is a number

            @arg        : obj expression
        */

        public static bool bIsNumber( object expression )
        {
            double retNum;
            bool isNum = Double.TryParse(Convert.ToString( expression ), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum );

            return isNum;
        }

        /*
            Read Stream to Bytes
        */

        public static byte[] FSReadFull( Stream input )
        {
            byte[] buffer = new byte[ 16 * 1024 ];
            using ( MemoryStream ms = new MemoryStream( ) )
            {
                int read;
                while ( ( read = input.Read( buffer, 0, buffer.Length ) ) > 0 )
                {
                    ms.Write( buffer, 0, read );
                }
                return ms.ToArray( );
            }
        }


        public string StringJoin(string[] array)
        {
            return string.Join(string.Empty, array);
        }

        public static string StringLast( string str, int len_tail )
        {
           if(len_tail >= str.Length) return str;
           return str.Substring(str.Length - len_tail);
        }

        public static bool StringValidateEmail( string str )
        {                       
            return Regex.IsMatch(str, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }

        /*
            Execute powershell query
            checks to see if a target file has been signed with x509 cert

            @param      : str query
            @return     : str
        */

        #region "Method: Exec Powershell Queries"

            static public string PowershellQ( string[] queries, bool bShowOutput = false )
            {
                using ( PowerShell ps = PowerShell.Create( ) )
                {
                    for ( int i = 0; i < queries.Length; i++ ) 
                    {
                        if ( bShowOutput )
                            Console.WriteLine( "PowerShell Query: " + queries[ i ] );

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

                            if ( bShowOutput )
                                Console.WriteLine( PSItem.ToString( ) );
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

            /*
                @todo       : deprecate in favor of FindProgramPath
            */

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
                        p.OutputDataReceived                += ( s, e ) => RouteOutput( e.Data );
                        p.Start( );
                        p.BeginOutputReadLine( );
                        p.WaitForExit( );

                        return p.ExitCode == 0;
                    }
                }
                catch( Win32Exception )
                {
                    throw new Exception( "Failed to locate app. 'where' command is not on PATH" );
                }
            }

            /*
                Environment Variables
            */

            public static string FindProgramPath( string filename )
            {
                var paths       = new[]{ Environment.CurrentDirectory }.Concat( Environment.GetEnvironmentVariable( "PATH" ).Split( ';' ) );
                var file_ext    = new[]{ String.Empty }.Concat(Environment.GetEnvironmentVariable( "PATHEXT" ).Split( ';' ).Where( e => e.StartsWith( "." ) ) );
                var combos      = paths.SelectMany( x => file_ext, ( path, file_ext ) => Path.Combine( path, filename + file_ext ) );

                return combos.FirstOrDefault( File.Exists );
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

    }

    /*
        Class > Initialize String Format

        adds additional functionality to String.Format for various character changes.
    */

    public class InitSF : IFormatProvider, ICustomFormatter
    {

        /*
            Get Format

            @ret        : obj
        */

        public object GetFormat( Type ftype )
        {
            if ( ftype == typeof( ICustomFormatter ) )
                return this;
            else
                return null;
        }

        /*
            Format

            adds a series of extra properties for string.format

            @usage      : String.Format( new InitSF( ), "text" );

            @arg        : str flag
            @arg        : obj arg
            @arg        : ifp provider
            @ret        : str
        */

        public string Format( string flag, object arg, IFormatProvider provider )
        {
            string obj = arg.ToString( );
            switch ( flag?.ToUpperInvariant( ) )
            {
                case "F":
                    return Helpers.ucfirst( obj );
                case "U":
                    return obj.ToUpper( );
                case "L":
                    return obj.ToLower( );
                default:
                    return obj;
            }
        }

    }

}

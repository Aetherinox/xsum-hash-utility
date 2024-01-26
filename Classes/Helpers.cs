using System;
using System.Diagnostics;
using System.Drawing;
using System.Management.Automation.Language;
using System.Security.Policy;

namespace xsum
{

    /*
        Class > Helpers
    */

    public class Helpers
    {

        static public void c0( string msg )
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

            Process clipboardExecutable     = new Process( ); 
            clipboardExecutable.StartInfo   = new ProcessStartInfo
            {
                RedirectStandardInput       = true,
                FileName                    = @"clip",
                UseShellExecute             = false
            };

            clipboardExecutable.Start( );
            clipboardExecutable.StandardInput.Write( str );
            clipboardExecutable.StandardInput.Close( ); 

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

            @arg        : str str
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
        */

        public static bool bIsNumber( object Expression )
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

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

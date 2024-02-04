#region "Using"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Reflection;

#endregion

namespace XSum
{

    /*
        Class > Strong Names
    */

    public class SN
    {
    
        /*
            Get Key

            Takes a pfx private key and extracts the Strong Name .snk public key
            PFX file should be placed in the VS Project output / bin folder.

            @arg        : str PfxFile
            @arg        : str PfxPasswd
            @arg        : str SnkOutput
            @ret        : void
        */

        public void GetKey( string PfxFile, string PfxPasswd, string SnkOutput = "aetherx.snk" )
        {

            if ( !File.Exists( PfxFile ) )
                Console.WriteLine( String.Format( "Cannot locate PFX key file {0}", PfxFile ) );

            X509Certificate2 cert               = new X509Certificate2( PfxFile, PfxPasswd, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet );
            RSACryptoServiceProvider provider   = (RSACryptoServiceProvider)cert.PrivateKey;

            byte[] array = provider.ExportCspBlob( !provider.PublicOnly );

            using ( FileStream fs = new FileStream( SnkOutput, FileMode.Create, FileAccess.Write ) )
            {
                fs.Write( array, 0, array.Length );
            }
        }

        /*
            Get Strongname Public Key (.snk file)
        */

        public static byte[] GetPublicKey( byte[] SnkFile )
        {
            var snkp            = new StrongNameKeyPair( SnkFile );
            byte[] key_pub      = snkp.PublicKey;

            return key_pub;
        }

        /*
            Get Strongname Public Token (.snk file)

            byte[] snk              = File.ReadAllBytes( "aetherx.snk" );
            byte[] publicKey        = GetPublicKey( snk );
            byte[] publicKeyToken   = GetPublicKeyToken( publicKey );

            string str_key          = XSum.SN.Read( publicKey );
            string str_token        = XSum.SN.Read( publicKeyToken );

        */

        public static byte[] GetPublicKeyToken( byte[] KeyPublic )
        {
            using ( var csp = new SHA1CryptoServiceProvider( ) )
            {
                byte[] hash     = csp.ComputeHash( KeyPublic );
                byte[] token    = new byte[ 8 ];

                for ( int i = 0; i < 8; i++ )
                {
                    token[ i ] = hash[ hash.Length - i - 1 ];
                }

                return token;
            }
        }

        /*
            Read > Bytes to String

            @arg        : byte a
            @ret        : str
        */

        public static string Read( byte[] a ) => string.Concat( a.Select( x => x.ToString( "X2" ).ToUpper( ) ) );

    }
}

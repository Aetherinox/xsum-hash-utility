
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
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Linq;
using XSum;

namespace Starksoft.Aspen.GnuPG
{

    /*
        Item Types
    */

    public enum OutputTypes
    {
        // Ascii armor output.
        AsciiArmor,

        // Binary output.
        Binary
    };

    /*
        Output Signature Type
    */

    public enum OutputSignatureTypes
    {
        // Make a clear text signature
        ClearText,

        // Make a detached signature.
        Detached,

        // Make a signature.
        Signature
    };

    /*

        Encrypt

            Create a new GnuPG object
                Gpg gpg = new Gpg();

            Specify a recipient that is already on the key-ring 
                gpg.Recipient = "myfriend@domain.com";

            Create an IO.Stream object to the source of the data and open it
                FileStream sourceFile = new FileStream(@"c:\temp\source.txt", FileMode.Open

            Create an IO.Stream object to a where I want the encrypt data to go
                 FileStream outputFile = new FileStream(@"c:\temp\output.txt", FileMode.Create);

            Encrypt the data using IO Streams - any type of input and output IO Stream can be used
            as long as the source (input) stream can be read and the destination (output) stream

            Can be written to
                gpg.Encrypt(sourceFile, outputFile);

            Close the files
                sourceFile.Close( );
                outputFile.Close( );

        Decrypt:

            Create a new GnuPG object
                Gpg gpg = new Gpg();

            Create an IO.Stream object to the encrypted source of the data and open it 
                FileStream encryptedFile = new FileStream(@"c:\temp\output.txt", FileMode.Open);

            Create an IO.Stream object to a where you want the decrypted data to go
                FileStream unencryptedFile = new FileStream(@"c:\temp\unencrypted.txt", FileMode.Create);

            Specify our secret passphrase (if we have one)
                gpg.Passphrase = "secret passphrase";

            Decrypt the data using IO Streams - any type of input and output IO Stream can be used
            as long as the source (input) stream can be read and the destination (output) stream 
            can be written to
                gpg.Decrypt(encryptedFile, unencryptedFile);

            Close the files
                encryptedFile.Close( );
                unencryptedFile.Close( );
    */

    public class Gpg : IDisposable
    {

        /*
            Define > Classes
        */

        static AppInfo AppInfo             = new AppInfo( );
        static Helpers Helpers             = new Helpers( );

        /*
            Define > Defaults
        */

        private const long DEFAULT_COPY_BUFFER_SIZE = 4096;
        private const int DEFAULT_TIMEOUT_MS = 10000; // 10 seconds
        private const OutputTypes DEFAULT_OUTPUT_TYPE = OutputTypes.AsciiArmor;
        private const OutputSignatureTypes DEFAULT_SIGNATURE_TYPE = OutputSignatureTypes.Signature;
        private string _passphrase;
        private string _recipient;
        private string _localUser;
        private string _homePath;
        private string _binaryPath;
        private OutputTypes _outputType = DEFAULT_OUTPUT_TYPE;
        private int _timeout = DEFAULT_TIMEOUT_MS;
        private Process _proc;
        private OutputSignatureTypes _outputSignatureType = DEFAULT_SIGNATURE_TYPE;
        private string _filename;
        private long _copyBufferSize = DEFAULT_COPY_BUFFER_SIZE;
        private bool _ignoreErrors;
        private string _userOptions;
        private Stream _outputStream;
        private Stream _errorStream;
        private Encoding _defaultEncoding = null;
        private Encoding _loadedStreamEncoding;

        /*   
            Action Types
        */

        private enum ActionTypes
        {
            Encrypt,
            Decrypt,
            Sign,
            Verify,
            SignEncrypt,
            Import
        };

        /*
            GPG > Initialize
            
            Create new object
                Gpg gpg = new Gpg( );
        */

        public Gpg( ) { }

        /*
            Initializes a new instance of GPG with specified GPG binary path

            :   gpg_path_exe
                Full path to the gpg, gpg2, gpg.exe or gpg2.exe executable binary

            :   gpg_path_home
                home directory where files to encrypt and decrypt are located.

        */

        public Gpg( string gpg_path_exe, string gpg_path_home )
        {
            _binaryPath     = gpg_path_exe;
            _homePath       = gpg_path_home;
        }

        /*
            Initializes a new instance of GPG with specified GPG binary path

            :   gpg_path_exe
                Full path to the gpg, gpg2, gpg.exe or gpg2.exe executable binary.
        */

        public Gpg( string gpg_path_exe )
        {
            _binaryPath     = gpg_path_exe;
        }

        /*
            Set Timeout

            Gets or sets the timeout value for the GnuPG operations in milliseconds.
            The default timeout is 10000 milliseconds (10 seconds).
        */

        public int Timeout
        {
            get { return ( _timeout ); }
            set { _timeout = value; }
        }

        /*
            Set Recipient

            Gets or set the recipient name to an associated public key.  This selected key will be used to encrypt data.
        */

        public string Recipient
        {
            get { return _recipient; }
            set { _recipient = value; }
        }

        /*
            Local User

            Gets or set the local user name to an associated private key. This selected key will be used to sign or decrypt data.
        */

        public string LocalUser
        {
            get { return _localUser; }
            set { _localUser = value; }
        }

        /*
            Passphrase

            Gets or set the secret passphrase text value used to gain access to the secret key.
        */

        public string Passphrase
        {
            get { return _passphrase; }
            set { _passphrase = value; }
        }

        /*
            OutputType

            Gets or sets the output type that GPG should generate.
        */

        public OutputTypes OutputType
        {
            get { return _outputType; }
            set { _outputType = value; }
        }

        /*
            HomePath

            Gets or sets a specific user home path if the default home path should not be used.
        */

        public string HomePath
        {
            get { return _homePath; }
            set { _homePath = value; }
        }

        /*
            BinaryPath

            Gets or sets the full path to the gpg or gpg2 binary on the system.
        */

        public string BinaryPath
        {
            get { return _binaryPath; }
            set { _binaryPath = value; }
        }

        /*
            OutputSignatureTypes

            Gets or sets the output signature that GPG should generate.
        */

        public OutputSignatureTypes OutputSignatureType
        {
            get { return _outputSignatureType; }
            set { _outputSignatureType = value; }
        }

        /*
            Filename

            Gets or sets the arg --set-filename so that the name of the file is embedded in the encrypted gpg blob.
        */

        public string Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        /*
            Copy Buffer Size

            Gets or sets the size of the copy buffer.  This value can be adjusted for performance when
            operating on large files.

            The size of the copy buffer.
        */

        public long CopyBufferSize
        {
            get { return _copyBufferSize; }
            set { _copyBufferSize = value; }
        }

        /*
            IgnoreErrors

            Gets or sets a value indicating whether execution errors reported by gpg should be ignored.

            true    : ignore errors
        */

        public bool IgnoreErrors
        {
            get { return _ignoreErrors; }
            set { _ignoreErrors = value; }
        }

        /*
            User Options

            Gets or sets user specified option GPG CLI argument string for any additional GPG options 
            that need to be specified by the user.

            String containing GPGP user command line options
        */

        public string UserOptions
        {
            get { return _userOptions; }
            set { _userOptions = value; }
        }

        /*
            Default Encoding

            Set to console default when you are using wpf app to avoid problem with recoding
            if console default is different than WPF default

            String containing GPGP user command line options
        */

        public Encoding DefaultEncoding
        {
            get { return _defaultEncoding; }
            set { _defaultEncoding = value; }
        }

        /*
            Throw Exceptions

            Throw exceptions when streams are not valid

            :   StreamInput
            :   StreamOutput
        */

        private void ValidateInputOutputStreams( Stream StreamInput, Stream StreamOutput )
        {
            if ( StreamInput == null )
                throw new ArgumentNullException( "Argument StreamInput can not be null." );

            if ( StreamOutput == null )
                throw new ArgumentNullException( "Argument StreamOutput can not be null." );

            if ( !StreamInput.CanRead )
                throw new ArgumentException( "Argument StreamInput must be readable." );

            if ( !StreamOutput.CanWrite )
                throw new ArgumentException( "Argument StreamOutput must be writable." );
        }

        /*
            ValidateInputStream

            Throw exceptions when stream is not valid 

            :   StreamInput
        */

        private void ValidateInputStream( Stream StreamInput )
        {
            if ( StreamInput == null )
                throw new ArgumentNullException("Argument StreamInput can not be null.");

            if ( !StreamInput.CanRead )
                throw new ArgumentException("Argument StreamInput must be readable.");
        }

        /*
            Sign & Encrypt

            Sign + encrypt data using the gpg executable with --sign arg.  Input data is provide via a stream.  Output
            data is returned as a stream.  Note that MemoryStream is supported for use.

            :   StreamInput
                Input stream data containing the data to encrypt.

            :   StreamOutput
                Output stream which will contain encrypted data.
        */

        public void SignAndEncrypt( Stream StreamInput, Stream StreamOutput )
        {
            ValidateInputOutputStreams( StreamInput, StreamOutput );
            ExecuteGpg( ActionTypes.SignEncrypt, StreamInput, StreamOutput );
        }

        /*
            Encrypt

            Encrypt data using the gpg executable.
            Input data is provide via a stream.
            Output data is returned as a stream.
            Note that MemoryStream is supported for use.

            :   StreamInput
                Input stream data containing the data to encrypt.

            :   StreamOutput
                Output stream which will contain encrypted data.

            You must add the recipient's public key to your GnuPG key ring before calling this method.
            Please see the GnuPG documentation for more information.
        */

        public void Encrypt( Stream StreamInput, Stream StreamOutput )
        {
            ValidateInputOutputStreams( StreamInput, StreamOutput );
            ExecuteGpg( ActionTypes.Encrypt, StreamInput, StreamOutput );
        }

        /*
            Decrypt

            Decrypt OpenPGP data using IO Streams.

            :   StreamInput
                Input stream containing encrypted data

            :   StreamOutput
                Output stream which will contain decrypted data.

            You must have the appropriate secret key on the GnuPG key ring before calling this method.
            Please see the GnuPG documentation for more information.
        */

        public void Decrypt(Stream StreamInput, Stream StreamOutput)
        {
            ValidateInputOutputStreams( StreamInput, StreamOutput );
            ExecuteGpg(ActionTypes.Decrypt, StreamInput, StreamOutput);
        }

        /*
            Sign

            Sign input stream data with secret key.

            :   StreamInput
                Input stream containing data to sign

            :   StreamOutput
                Output stream containing signed data.

            You must have the appropriate secret key on the GnuPG key ring before calling this method.
            Please see the GnuPG documentation for more information.
        */

        public void Sign(Stream StreamInput, Stream StreamOutput)
        {
            ValidateInputOutputStreams(StreamInput, StreamOutput);
            ExecuteGpg(ActionTypes.Sign, StreamInput, StreamOutput);
        }

        /*
            Verify

            Verify signed input stream data with default user key.

            :   StreamInput
                Input stream containing signed data to verify.
        */

        public void Verify( Stream StreamInput )
        {
            ValidateInputStream( StreamInput );

            if ( StreamInput.Position == StreamInput.Length )
                throw new ArgumentException("Argument StreamInput position cannot be set to the end.  Nothing to read.");

            ExecuteGpg( ActionTypes.Verify, StreamInput, new MemoryStream( ) );
        }

        /*
            GPG > Verify

            :   StreamInput
                Input stream containing data to sign

            :   StreamOutput
                Output stream containing signed data.

            @returns string, string

            FileStream fileStream   = new FileStream( "file.txt.asc", FileMode.Open, FileAccess.Read, FileShare.Read );
            MemoryStream outputVer  = new MemoryStream();

            var result              = gpg.Verify( fileStream, outputVer );
            string user_key         = result.user_key;
            string user_name        = result.user_name;

            outputVer.Position      = 0;
            StreamReader reader2    = new StreamReader(outputVer);
            string text2            = reader2.ReadToEnd();
        */

        public (string user_key, string user_name) Verify( Stream StreamInput, Stream StreamOutput )
        {
            ValidateInputOutputStreams( StreamInput, StreamOutput );
            StreamInput.Position    = 0;

            ExecuteGpg( ActionTypes.Verify, StreamInput, StreamOutput );

            StreamOutput.Position   = 0;
            StreamReader reader     = new StreamReader( StreamOutput, _loadedStreamEncoding );
            string text             = reader.ReadToEnd( );

            string user_key         = Verify_GetKeyID( text );
            string user_name        = Verify_GetUserName( text );

            return ( user_key, user_name );
        }

        /*
            Verify > Get Key ID

            Returns the GPG key ID
        */

        private string Verify_GetKeyID( string reader )
        {
            if ( string.IsNullOrWhiteSpace( reader ) ) return null;

            var lines_key           = reader.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None );
            string gpg_key          = lines_key[ 1 ].Split( ' ' ).Last( );
            gpg_key                 = Helpers.StringLast( gpg_key, 16 );

            //Regex regex_key       = new Regex( @"\b(?i)Using [A-Za-z]+ key\b" );
            //var output_key        = regex_key.Replace( lines_key[ 1 ], string.Empty );
            //output_key            = Regex.Replace( output_key, @"\s+", "");
            //output_key            = output_key.Remove(0, 4);

            if ( IsCorrectHashKey( gpg_key ) )
                return gpg_key;

            return null;
        }

        /*
            Verify > Get User ID

            GPG > Get Key ID / Email
        */

        private string Verify_GetUserName( string reader )
        {
            if (string.IsNullOrWhiteSpace( reader ) ) return null;

            var lines_id        = reader.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None );
            string gpg_id       = lines_id[ 2 ];
            Match output        = Regex.Match( gpg_id, "\"([^\"]*)\"" );
            string result       = output.Value.Replace( "\"", string.Empty );


            if ( !String.IsNullOrEmpty( result ) )
                return result;

            return null;
        }

        /*
            Check if keyId is correct key

            :   keyId
                key to check
        */

        public static bool IsCorrectHashKey( string keyId )
        {
            if ( string.IsNullOrWhiteSpace( keyId ) ) return false;

            const string hashRegex  = "^[a-fA-F0-9]+$";
            Match match             = Regex.Match( keyId, hashRegex );

            return match.Success;
        }

        /*
            Retrieves a collection of all secret keys.
            Collection of GnuPGKey objects.
        */

        public GpgKeyCollection GetSecretKeys( )
        {
            return new GpgKeyCollection( ExecuteGpgNoIO( "--list-secret-keys" ) );
        }

        /*
            Retrieves a collection of all public keys.
            Collection of GnuPGKey objects.
        */

        public GpgKeyCollection GetKeys( )
        {
            return new GpgKeyCollection( ExecuteGpgNoIO( "--list-keys" ) );
        }

        /*
            Import public key

            :   StreamInput
                Input stream containing public key.

            @return Name of imported key.
        */

        public string Import(Stream StreamInput)
        {
            if (StreamInput == null)
                throw new ArgumentNullException("Argument StreamInput can not be null.");

            if (!StreamInput.CanRead)
                throw new ArgumentException("Argument StreamInput must be readable.");

            using (Stream StreamOutput = new MemoryStream())
            {
                ExecuteGpg(ActionTypes.Import, StreamInput, StreamOutput);
                StreamOutput.Seek(0, SeekOrigin.Begin);
                StreamReader sr;
                if (DefaultEncoding == null)
                {
                    sr = new StreamReader(StreamOutput);
                }
                else
                {
                    sr = new StreamReader(StreamOutput, DefaultEncoding);
                }
              
                string line;
                while ((line = sr.ReadLine()) != null)
                {

                    /*
                        Example Output
                        Output looks like this:

                        gpg: key 13F1C2BB58E7940B: public key \"Joe Test <joe@domain.com>\" imported
                        gpg: key FF5176CC: public key "One Team <user@domain.com>" imported
                    */

                    Match m = Regex.Match(line, @"imported|not changed");
                    if (m.Success)
                    {
                        return m.Groups[1].Value;
                    }
                }
            }

            throw new GpgException("Unable to identify name of imported key.  Possible import error or unrecognized text output.");
        }

        /*
            Get PGP version

            Gets the GPG binary version number as reported by the executable.
        */

        public GpgVersion GetGpgVersion( )
        {
            GpgVersion ver = GpgVersionParser.Parse(ExecuteGpgNoIO( "--version" ) );

            return ver;
        }


        /*
            Execute GPG No ID

            Executes the gpg program as a single command without input or output streams.
            This method is used to report data back from gpg such as key list information.
        
            :   gpgArgs
                gpg command arguments to pass to gpg executable

            :   return
                The gpg command
        */


        private StreamReader ExecuteGpgNoIO( string gpgArgs )
        {
            StringBuilder options   = new StringBuilder( );
            options.Append  ( gpgArgs );
            options.Append  ( " " ); // append a space to allow for additional commands to be added if required

            //  set a home directory if the user specifies one
            if ( !String.IsNullOrEmpty(_homePath ) )
                options.Append(String.Format( CultureInfo.InvariantCulture, "--homedir \"{0}\" ", _homePath ) );

            //  create a process info object with command line options
            ProcessStartInfo procInfo = new ProcessStartInfo( GetGpgBinaryPath(), options.ToString( ) );

            //  init the procInfo object
            procInfo.CreateNoWindow         = true;
            procInfo.UseShellExecute        = false;
            procInfo.RedirectStandardInput  = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError  = true;

            if (DefaultEncoding != null)
            {
                procInfo.StandardErrorEncoding  = DefaultEncoding;
                procInfo.StandardOutputEncoding = DefaultEncoding;
            }

            // create a memory stream to hold the output in memory
            MemoryStream StreamOutput = new MemoryStream();

            try
            {
                //  start the gpg process and get back a process start info object
                _proc = Process.Start( procInfo );
                _proc.StandardInput.Flush( );

                //  wait for the process to return with an exit code (with a timeout variable)
                if ( !_proc.WaitForExit( Timeout ) )
                    throw new GpgException("A time out event occurred while executing the GPG program.");

                //  if the process exit code is not 0 then read the error text from the gpg.exe process and throw an exception
                if ( _proc.ExitCode != 0 )
                    throw new GpgException(_proc.StandardError.ReadToEnd());

                // grab a copy of the console output
                CopyStream(_proc.StandardOutput.BaseStream, StreamOutput);
            }
            catch (Exception exp)
            {
                throw new GpgException(String.Format("An error occurred while trying to execute command {0}: {1}.", gpgArgs, exp));
            }
            finally
            {
                Dispose(true);
            }

            // reset the stream position and return as a StreamReader
            StreamReader reader;

            if (DefaultEncoding == null)
                reader = new StreamReader(StreamOutput);

            else
                reader = new StreamReader(StreamOutput, DefaultEncoding);

            reader.BaseStream.Position = 0;
            return reader;
        }

        private string GetGpgArgs(ActionTypes action)
        {
            // validate input
            switch (action)
            {
                case ActionTypes.Encrypt:
                case ActionTypes.SignEncrypt:
                    if (String.IsNullOrEmpty(_recipient))
                        throw new GpgException("A Recipient is required before encrypting data.  Please specify a valid recipient using the Recipient property on the GnuPG object.");
                    break;
            }

            StringBuilder options = new StringBuilder();

            //  set a home directory if the user specifies one
            if (!String.IsNullOrEmpty(_homePath))
                options.Append(String.Format(CultureInfo.InvariantCulture, "--homedir \"{0}\" ", _homePath));

            //  tell gpg to read the passphrase from the standard input so we can automate providing it
            options.Append("--passphrase-fd 0 ");

            // if gpg cli version is >= 2.1 then instruct gpg not to prompt for a password
            // by specifying the pinetnry-mode argument
            GpgVersion ver = GetGpgVersion();
            if ((ver.Major == 2 && ver.Minor >= 1) || ver.Major >= 3)
            {
                options.Append("--pinentry-mode loopback ");
            }

            //  turn off verbose statements
            options.Append("--no-verbose ");

            // use batch mode and never ask or allow interactive commands. 
            options.Append("--batch ");

            //  always use the trusted model so we don't get an interactive session with gpg.exe
            options.Append("--trust-model always ");

            // if provided specify the key to use by local user name
            if (!String.IsNullOrEmpty(_localUser))
                options.Append(String.Format(CultureInfo.InvariantCulture, "--local-user {0} ", _localUser));

            // if provided specify the recipient key to use by recipient user name
            if (!String.IsNullOrEmpty(_recipient))
                options.Append(String.Format(CultureInfo.InvariantCulture, "--recipient {0} ", _recipient));

            // add any user specific options if provided
            if (!String.IsNullOrEmpty(_userOptions))
                options.Append(_userOptions);

            //  handle the action
            switch (action)
            {
                case ActionTypes.Encrypt:
                    if (_outputType == OutputTypes.AsciiArmor)
                        options.Append("--armor ");

                    // if a filename needs to be embedded in the encrypted blob, set it
                    if (!String.IsNullOrEmpty(_filename))
                        options.Append(String.Format(CultureInfo.InvariantCulture, "--set-filename \"{0}\" ", _filename));

                    options.Append("--encrypt ");
                    break;

                case ActionTypes.Decrypt:
                    options.Append("--decrypt ");
                    break;

                case ActionTypes.Sign:
                    switch (_outputSignatureType)
                    {
                        case OutputSignatureTypes.ClearText:
                            options.Append("--clearsign ");
                            break;

                        case OutputSignatureTypes.Detached:
                            options.Append("--detach-sign ");
                            break;

                        case OutputSignatureTypes.Signature:
                            options.Append("--sign ");
                            break;
                    }
                    break;
                case ActionTypes.SignEncrypt:
                    if (_outputType == OutputTypes.AsciiArmor)
                        options.Append("--armor ");

                    // if a filename needs to be embedded in the encrypted blob, set it
                    if (!String.IsNullOrEmpty(_filename))
                        options.Append(String.Format(CultureInfo.InvariantCulture, "--set-filename \"{0}\" ", _filename));

                    // determine which type of signature to generate
                    switch (_outputSignatureType)
                    {
                        case OutputSignatureTypes.ClearText:
                            options.Append("--clearsign ");
                            break;

                        case OutputSignatureTypes.Detached:
                            options.Append("--detach-sign ");
                            break;

                        case OutputSignatureTypes.Signature:
                            options.Append("--sign ");
                            break;
                    }

                    options.Append("--encrypt ");
                    break;

                case ActionTypes.Verify:
                    options.Append("--verify ");
                    break;

                case ActionTypes.Import:
                    options.Append("--import ");
                    break;
            }

            return options.ToString();
        }

        /// <summary>
        /// Executes the gpg program with the correct command line args based on the selected crypto action
        /// and feeds the StreamInput data to the program and returns the output data as an StreamOutput.
        /// </summary>
        /// <param name="action">Action to perform (sign, encrypt, etc).</param>
        /// <param name="StreamInput">Input stream.</param>
        /// <param name="StreamOutput">Output stream.</param>
        private void ExecuteGpg( ActionTypes action, Stream StreamInput, Stream StreamOutput )
        {
            string gpgErrorText             = string.Empty;
            string gpgPath                  = GetGpgBinaryPath( );
            string gpgArgs                  = GetGpgArgs( action );

            //  create a process info object with command line options
            ProcessStartInfo procInfo       = new ProcessStartInfo( gpgPath, gpgArgs );

            //  init the procInfo object
            procInfo.CreateNoWindow         = true;
            procInfo.UseShellExecute        = false;
            procInfo.RedirectStandardInput  = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.RedirectStandardError  = true;

            if (DefaultEncoding != null)
            {
                procInfo.StandardErrorEncoding  = DefaultEncoding;
                procInfo.StandardOutputEncoding = DefaultEncoding;
            }

            try
            {
                //  start the gpg process and get back a process start info object

                _proc = Process.Start(procInfo);
                _proc.StandardInput.WriteLine(_passphrase);
                _proc.StandardInput.Flush( );

                _outputStream           = StreamOutput;
                _errorStream            = new MemoryStream();
                _loadedStreamEncoding   = _proc.StandardError.CurrentEncoding;

                // set up threads to run the output stream and error stream asynchronously
                ThreadStart outputEntry = new ThreadStart(AsyncOutputReader);
                Thread outputThread     = new Thread(outputEntry);
                outputThread.Name       = "gpg stdout";
                outputThread.Start();

                ThreadStart errorEntry  = new ThreadStart(AsyncErrorReader);
                Thread errorThread      = new Thread(errorEntry);
                errorThread.Name        = "gpg stderr";
                errorThread.Start();

                //  copy the input stream to the process standard input object
                CopyStream(StreamInput, _proc.StandardInput.BaseStream);

                _proc.StandardInput.Flush();

                // close the process standard input object
                _proc.StandardInput.Close();

                //  wait for the process to return with an exit code (with a timeout variable)
                if (!_proc.WaitForExit(_timeout))
                {
                    throw new GpgException("A time out event occurred while executing the GPG program.");
                }

                if ( !outputThread .Join(_timeout / 2 ) )
                    outputThread.Abort();

                if ( !errorThread.Join( _timeout / 2 ) )
                    errorThread.Abort();

                //  if the process exit code is not 0 then read the error text from the gpg.exe process 
                if (_proc.ExitCode != 0 && !_ignoreErrors)
                {
                    StreamReader rerror = new StreamReader(_errorStream, _loadedStreamEncoding);
                    _errorStream.Position = 0;
                    gpgErrorText = rerror.ReadToEnd();
                }


                // key name and verification output are output to error stream so read from the error stream and write out
                // to the output stream
                if (action == ActionTypes.Import || action == ActionTypes.Verify)
                {
                    _errorStream.Position = 0;
                    byte[] buffer = new byte[4048];
                    int count;
                    while ((count = _errorStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        StreamOutput.Write(buffer, 0, count);
                    }
                }

            }
            catch (Exception exp)
            {
                throw new GpgException(String.Format(CultureInfo.InvariantCulture, "Error.  Action: {0}.  Command args: {1}", action.ToString(), procInfo.Arguments), exp);
            }
            finally
            {
                Dispose();
            }

            // throw an exception with the error information from the gpg.exe process
            if (gpgErrorText.IndexOf("bad passphrase") != -1)
                throw new GpgBadPassphraseException(gpgErrorText);

            if (gpgErrorText.Length > 0)
                Console.WriteLine( gpgErrorText );
                //throw new GpgException(gpgErrorText);
        }

        private string GetGpgBinaryPath()
        {
            if (String.IsNullOrEmpty(_binaryPath))
                throw new GpgException("gpg binary path not set");

            if (!File.Exists(_binaryPath))
                throw new GpgException(String.Format("gpg binary path executable invalid or file permissions do not allow access: {0}", _binaryPath));

            return _binaryPath;
        }

        private void CopyStream(Stream input, Stream output)
        {
            if (_asyncWorker != null && _asyncWorker.CancellationPending)
                return;

            byte[] bytes = new byte[_copyBufferSize];
            int i;
            while ((i = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                if (_asyncWorker != null && _asyncWorker.CancellationPending)
                    break;
                output.Write(bytes, 0, i);
            }
        }

        private void AsyncOutputReader()
        {
            Stream input = _proc.StandardOutput.BaseStream;
            Stream output = _outputStream;

            byte[] bytes = new byte[_copyBufferSize];
            int i;
            while ((i = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                output.Write(bytes, 0, i);
            }
        }

        private void AsyncErrorReader()
        {
            Stream input    = _proc.StandardError.BaseStream;
            Stream output   = _errorStream;

            byte[] bytes = new byte[_copyBufferSize];
            int i;
            while ((i = input.Read(bytes, 0, bytes.Length)) != 0)
            {
                output.Write(bytes, 0, i);
            }

        }

        /// <summary>
        /// Dispose method for the GnuPG inteface class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method for the GnuPG interface class.
        /// </summary>       
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_proc != null)
                {
                    //  close all the streams except for our the output stream
                    _proc.StandardInput.Close();
                    _proc.StandardOutput.Close();
                    _proc.StandardError.Close();
                    _proc.Close();
                }
            }

            if (_proc != null)
            {
                _proc.Dispose();
                _proc = null;
            }
        }

        /*
            Destructor method for the GnuPG interface class.
        */

        ~Gpg( )
        {
            Dispose( false );
        }

        #region Asynchronous Methods

        private BackgroundWorker _asyncWorker;
        private Exception _asyncException;
        bool _asyncCancelled;

        /*
            Is Busy

            Gets a value indicating whether an asynchronous operation is running.
            Returns true if an asynchronous operation is running; otherwise, false.
        */

        public bool IsBusy
        {
            get { return _asyncWorker == null ? false : _asyncWorker.IsBusy; }
        }

        /*
            ASync > Check Cancelled

            Gets a value indicating whether an asynchronous operation is cancelled.
            Returns true if an asynchronous operation is cancelled; otherwise, false.
        */

        public bool IsAsyncCancelled
        {
            get { return _asyncCancelled; }
        }

        /*
            ASync > Cancel

            Cancels any asychronous operation that is currently active.
        */

        public void CancelAsync()
        {
            if ( _asyncWorker != null && !_asyncWorker.CancellationPending && _asyncWorker.IsBusy )
            {
                _asyncCancelled = true;
                _asyncWorker.CancelAsync( );
            }
        }

        /*
            ASync > Create Worker
        */

        private void CreateAsyncWorker( )
        {
            if ( _asyncWorker != null )
                _asyncWorker.Dispose( );

            _asyncException     = null;
            _asyncWorker        = null;
            _asyncCancelled     = false;
            _asyncWorker        = new BackgroundWorker( );
        }

        /*
            Event handler for EncryptAsync method completed.
        */

        public event EventHandler<EncryptAsyncCompletedEventArgs> EncryptAsyncCompleted;

        /*
            Starts asynchronous execution to encrypt OpenPGP data using IO Streams.

            :   StreamInput
                Input stream data containing the data to encrypt.

            :   StreamOutput
                Output stream which will contain encrypted data.
        
                You must add the recipient's public key to your GnuPG key ring before calling this method.  Please see the GnuPG documentation for more information.
        */

        public void EncryptAsync( Stream StreamInput, Stream StreamOutput )
        {
            if ( _asyncWorker != null && _asyncWorker.IsBusy )
                throw new InvalidOperationException("The GnuPG object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            CreateAsyncWorker();

            _asyncWorker.WorkerSupportsCancellation = true;
            _asyncWorker.DoWork                     += new DoWorkEventHandler( EncryptAsync_DoWork );
            _asyncWorker.RunWorkerCompleted         += new RunWorkerCompletedEventHandler( EncryptAsync_RunWorkerCompleted );

            Object[] args       = new Object[ 2 ];
            args[ 0 ]           = StreamInput;
            args[ 1 ]           = StreamOutput;

            _asyncWorker.RunWorkerAsync(args);
        }

        /*
            Encrypt > ASync > Do Work
        */

        private void EncryptAsync_DoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                Object[] args = ( Object[] )e.Argument;
                Encrypt( ( Stream )args[ 0 ], ( Stream )args[ 1 ] );
            }
            catch (Exception ex)
            {
                _asyncException = ex;
            }
        }

        /*
            Encrypt > ASync > Completed
        */

        private void EncryptAsync_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            if ( EncryptAsyncCompleted != null )
                EncryptAsyncCompleted( this, new EncryptAsyncCompletedEventArgs( _asyncException, _asyncCancelled ) );
        }

        /*
            Event handler for DecryptAsync completed.
        */

        public event EventHandler<DecryptAsyncCompletedEventArgs> DecryptAsyncCompleted;

        /*
            Starts asynchronous execution to decrypt OpenPGP data using IO Streams.

            :   StreamInput
                Input stream containing encrypted data.

            :   StreamOutput
                Output stream which will contain decrypted data.
        */

        public void DecryptAsync( Stream StreamInput, Stream StreamOutput )
        {
            if (_asyncWorker != null && _asyncWorker.IsBusy)
                throw new InvalidOperationException("The Gpg object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            CreateAsyncWorker( );

            _asyncWorker.WorkerSupportsCancellation     = true;
            _asyncWorker.DoWork                         += new DoWorkEventHandler( DecryptAsync_DoWork );
            _asyncWorker.RunWorkerCompleted             += new RunWorkerCompletedEventHandler(DecryptAsync_RunWorkerCompleted);

            Object[] args       = new Object [2 ];
            args[ 0 ]           = StreamInput;
            args[ 1 ]           = StreamOutput;

            _asyncWorker.RunWorkerAsync( args );
        }

        /*
            Decrypt > ASync > Do Work
        */

        private void DecryptAsync_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Object[] args = ( Object[] )e.Argument;
                Decrypt( ( Stream )args[ 0 ], ( Stream )args[ 1 ] );
            }
            catch ( Exception ex )
            {
                _asyncException = ex;
            }
        }

        /*
            Decrypt > ASync > Completed
        */

        private void DecryptAsync_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            if ( DecryptAsyncCompleted != null )
                DecryptAsyncCompleted( this, new DecryptAsyncCompletedEventArgs( _asyncException, _asyncCancelled ) );
        }

        /*
            Event handler for SignAsync completed.
        */

        public event EventHandler<SignAsyncCompletedEventArgs> SignAsyncCompleted;

        /*
            Starts asynchronous execution to Sign OpenPGP data using IO Streams.

            :   StreamInput
                Input stream containing data to sign.

            :   StreamOutput
                Output stream which will contain Signed data.
        */

        public void SignAsync( Stream StreamInput, Stream StreamOutput )
        {
            if ( _asyncWorker != null && _asyncWorker.IsBusy )
                throw new InvalidOperationException("The Gpg object is already busy executing another asynchronous operation.  You can only execute one asychronous method at a time.");

            CreateAsyncWorker( );

            _asyncWorker.WorkerSupportsCancellation = true;
            _asyncWorker.DoWork                     += new DoWorkEventHandler( SignAsync_DoWork );
            _asyncWorker.RunWorkerCompleted         += new RunWorkerCompletedEventHandler( SignAsync_RunWorkerCompleted );

            Object[ ] args      = new Object[ 2 ];
            args[ 0 ]           = StreamInput;
            args[ 1 ]           = StreamOutput;

            _asyncWorker.RunWorkerAsync( args );
        }

        /*
            Sign > ASync > Start
        */

        private void SignAsync_DoWork( object sender, DoWorkEventArgs e )
        {
            try
            {
                Object[] args = ( Object[] )e.Argument;
                Sign( ( Stream )args[ 0 ], ( Stream )args[ 1 ] );
            }
            catch ( Exception ex )
            {
                _asyncException = ex;
            }
        }

        /*
            Sign > ASync > Complete
        */

        private void SignAsync_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            if ( SignAsyncCompleted != null )
                SignAsyncCompleted( this, new SignAsyncCompletedEventArgs( _asyncException, _asyncCancelled ) );
        }

        #endregion

    }
}

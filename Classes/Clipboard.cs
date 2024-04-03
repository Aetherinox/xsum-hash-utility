using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace XSum
{

    static class Clipboard
    {
        public static void Copy( string text )
        {
            OpenClipboard   ( );
            EmptyClipboard  ( );

            IntPtr hGlobal = default;

            try
            {
                var bytes   = ( text.Length + 1 ) * 2 ;
                hGlobal     = Marshal.AllocHGlobal(bytes);

                if ( hGlobal == default )
                    ThrowError( );

                var target = GlobalLock(hGlobal);

                if (target == default)
                    ThrowError();

                try
                {
                    Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
                }
                finally
                {
                    GlobalUnlock(target);
                }

                if (SetClipboardData(cfUnicodeText, hGlobal) == default)
                    ThrowError( );

                hGlobal = default;
            }
            finally
            {
                if ( hGlobal != default )
                    Marshal.FreeHGlobal(hGlobal);

                CloseClipboard( );
            }
        }

        public static void OpenClipboard()
        {
            var num = 10;
            while (true)
            {
                if (OpenClipboard(default))
                    break;

                if (--num == 0)
                    ThrowError( );

                Thread.Sleep(100);
            }
        }

        const uint cfUnicodeText = 13;

        static void ThrowError( )
        {
            throw new Win32Exception( Marshal.GetLastWin32Error( ) );
        }

        [ DllImport( "kernel32.dll", SetLastError = true ) ]
        static extern IntPtr GlobalLock( IntPtr hMem );

        [ DllImport( "kernel32.dll", SetLastError = true ) ]
        [ return: MarshalAs( UnmanagedType.Bool ) ]
        static extern bool GlobalUnlock( IntPtr hMem );

        [ DllImport( "user32.dll", SetLastError = true ) ]
        [ return: MarshalAs( UnmanagedType.Bool ) ]
        static extern bool OpenClipboard( IntPtr hWndNewOwner );

        [ DllImport( "user32.dll", SetLastError = true ) ]
        [return: MarshalAs( UnmanagedType.Bool ) ]
        static extern bool CloseClipboard( );

        [ DllImport( "user32.dll", SetLastError = true ) ]
        static extern IntPtr SetClipboardData( uint uFormat, IntPtr data );

        [ DllImport( "user32.dll" ) ]
        static extern bool EmptyClipboard( );
    }
}

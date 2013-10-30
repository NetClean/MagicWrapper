using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;

// fileidentifier.native.dll is build from this project: https://github.com/glensc/file

// Releases of UNIX file can be found here: ftp://ftp.astron.com/pub/file/

namespace NetClean.MimeReader
{
    public class Magic : IDisposable
    {
        public class MagicException : System.Exception
        {
            override public string Message { 
                get{
                    return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", errno, error);
                } 
            }

            public MagicException(int errno, string error)
            {
                this.errno = errno;
                this.error = error;
            }
            public int errno;
            public string error;
        }

        public const int MAGIC_NONE = 0x000000; /* No flags */
        public const int MAGIC_DEBUG = 0x000001; /* Turn on debugging */
        public const int MAGIC_SYMLINK = 0x000002; /* Follow symlinks */
        public const int MAGIC_COMPRESS = 0x000004; /* Check inside compressed files */
        public const int MAGIC_DEVICES = 0x000008; /* Look at the contents of devices */
        public const int MAGIC_MIME_TYPE = 0x000010; /* Return the MIME type */
        public const int MAGIC_CONTINUE = 0x000020; /* Return all matches */
        public const int MAGIC_CHECK = 0x000040; /* Print warnings to stderr */
        public const int MAGIC_PRESERVE_ATIME = 0x000080; /* Restore access time on exit */
        public const int MAGIC_RAW = 0x000100; /* Don't translate unprintable chars */
        public const int MAGIC_ERROR = 0x000200; /* Handle ENOENT etc as real errors */
        public const int MAGIC_MIME_ENCODING = 0x000400; /* Return the MIME encoding */
        public const int MAGIC_MIME = (MAGIC_MIME_TYPE | MAGIC_MIME_ENCODING);
        public const int MAGIC_APPLE = 0x000800; /* Return the Apple creator and type */
        public const int MAGIC_NO_CHECK_COMPRESS = 0x001000; /* Don't check for compressed files */
        public const int MAGIC_NO_CHECK_TAR = 0x002000; /* Don't check for tar files */
        public const int MAGIC_NO_CHECK_SOFT = 0x004000; /* Don't check magic entries */
        public const int MAGIC_NO_CHECK_APPTYPE = 0x008000; /* Don't check application type */
        public const int MAGIC_NO_CHECK_ELF = 0x010000; /* Don't check for elf details */
        public const int MAGIC_NO_CHECK_TEXT = 0x020000; /* Don't check for text files */
        public const int MAGIC_NO_CHECK_CDF = 0x040000; /* Don't check for cdf files */
        public const int MAGIC_NO_CHECK_TOKENS = 0x100000; /* Don't check tokens */
        public const int MAGIC_NO_CHECK_ENCODING = 0x200000; /* Don't check text encodings */

        [DllImport("fileidentifier.native.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe IntPtr magic_open(int flags);

        [DllImport("fileidentifier.native.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern unsafe int magic_load(IntPtr m, string file);

        [DllImport("fileidentifier.native.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe private extern static IntPtr magic_file(IntPtr m, string filename);

        [DllImport("fileidentifier.native.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe private extern static void magic_close(IntPtr m);

        [DllImport("fileidentifier.native.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe private extern static IntPtr magic_error(IntPtr m);

        [DllImport("fileidentifier.native.dll", CallingConvention = CallingConvention.Cdecl)]
        unsafe private extern static int magic_errno(IntPtr m);

        [DllImport("kernel32.dll", SetLastError = true, CharSet=CharSet.Unicode)]
        public static extern int GetShortPathNameW(string lpszLongPath, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath, [MarshalAs(UnmanagedType.U4)] int cchBuffer);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetFullPathNameW(string lpFileName, [MarshalAs(UnmanagedType.U4)] int nBufferLength, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpBuffer, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpFilePart);

        unsafe IntPtr m;
        Object magicLock = new Object();

        public void Dispose()
        {
            Dispose(true);
        }

        unsafe protected virtual void Dispose(bool disposeManaged)
        {
            lock (magicLock)
            {
                if (m != IntPtr.Zero)
                {
                    magic_close(m);
                    m = IntPtr.Zero;
                }
            }

            if (disposeManaged) { }
        }

        ~Magic() {
            Dispose(false);
        }

        unsafe public string GetInfo(string file)
        {
            lock (magicLock)
            {
                if (m != IntPtr.Zero)
                {
                    IntPtr desc = magic_file(m, file);
                    if (desc != IntPtr.Zero)
                    {
                        return Marshal.PtrToStringAnsi(desc);
                    }
                    throw new MagicException(magic_errno(m), Marshal.PtrToStringAnsi(magic_error(m)));
                }

                throw new MagicException(-1, "Invalid magic handle");
            }
        }

        unsafe public Magic(string database, int flags)
        {
            lock (magicLock)
            {
                m = magic_open(flags | MAGIC_ERROR);

                if (m == IntPtr.Zero)
                {
                    throw new MagicException(-1, "Couldn't create magic handle");
                }

                int ret = magic_load(m, database);

                if (ret < 0)
                {
                    throw new MagicException(magic_errno(m), Marshal.PtrToStringAnsi(magic_error(m)));
                }
            }
        }
    }
}
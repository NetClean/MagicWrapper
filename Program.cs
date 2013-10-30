using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NetClean.MimeReader
{
	class Program
	{
		private static Magic fileMagic;
		private static Magic mimeMagic;

		private static StreamReader input;
        private static StreamWriter output;
        private static StreamWriter error;

		private static JavaScriptSerializer serializer;

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        [DllImport("kernel32.dll", SetLastError = true)] static extern ErrorModes SetErrorMode(ErrorModes uMode);
        
        static void Main(string[] args)
		{
            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS | ErrorModes.SEM_NOGPFAULTERRORBOX | ErrorModes.SEM_NOOPENFILEERRORBOX);

            fileMagic = new Magic(String.Format("{0};{1}", GetPath("ncmagic.list"), GetPath("magic.mgc")), Magic.MAGIC_NONE | Magic.MAGIC_NO_CHECK_CDF);
            mimeMagic = new Magic(String.Format("{0};{1}", GetPath("ncmagic.list"), GetPath("magic.mgc")), Magic.MAGIC_MIME_TYPE | Magic.MAGIC_NO_CHECK_CDF);

			input = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
            output = new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding);
            error = new StreamWriter(Console.OpenStandardError(), Console.OutputEncoding);

			serializer = new JavaScriptSerializer();

			if (args.Length == 1)
			{
				switch (args[0])
				{
					case "--stay_open":
						EndlessOperation();
						break;

					case "--help":
						PrintHelp();
						break;

					default:
						output.Write(GetInfoJSON(args[0]));
						if (!output.AutoFlush) output.Flush();
						break;
				}
			}
			else
			{
				PrintHelp();
			}
		}

		private static void EndlessOperation()
		{
			while (!input.EndOfStream)
			{
				String line = input.ReadLine().Trim();
				if (line == "exit")
					break;

				output.Write(GetInfoJSON(line));
				output.WriteLine("{ready}");
                if (!output.AutoFlush) output.Flush();
			}
		}

		private static String GetInfoJSON(String filepath) 
		{
            String mimeType;
			String fileType;

			try
			{
				mimeType = mimeMagic.GetInfo(filepath);
				fileType = fileMagic.GetInfo(filepath);
			}
			catch (Exception ex)
			{
                mimeType = "unknown";
				fileType = "Error reading file";
			}

			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.Append("{\n");
			stringBuilder.AppendFormat("  \"MimeType\": {0},\n", serializer.Serialize(mimeType));
			stringBuilder.AppendFormat("  \"FileType\": {0}\n", serializer.Serialize(fileType));
			stringBuilder.Append("}\n");

			return stringBuilder.ToString();
		}

		private static String GetPath(String fileName)
		{
			return Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, fileName);
		}

		private static void PrintHelp()
		{
			Console.Error.WriteLine("NetClean Magic Wrapper\n");
			Console.Error.WriteLine("  Usage: NetClean.MagicWrapper.exe [file]");
			Console.Error.WriteLine("           Get mimetype/filetype of single file\n");
			Console.Error.WriteLine("         NetClean.MagicWrapper.exe --stay_open");
			Console.Error.WriteLine("           Reads filename from stdin (one per line),");
			Console.Error.WriteLine("           and prints back results to stdout");
			Console.Error.WriteLine("           Each result is followed by a line containing \"{ready}\"\n");
			Console.Error.WriteLine("         NetClean.MagicWrapper.exe --help");
			Console.Error.WriteLine("           Displays this help");
		}
	}
}

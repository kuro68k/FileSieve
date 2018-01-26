using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Keio.Utils;

namespace FileSieve
{
	class Program
	{
		static string srcDir = string.Empty;
		static string destDir = string.Empty;
		static string pattern = string.Empty;
		static bool moveFiles = false;
		static bool overwrite = false;
		static bool preview = false;
		static bool quiet = false;
		static bool continueOnError = false;

		static void Main(string[] args)
		{
			CmdArgs argProcessor = new CmdArgs()
			{
				{ new CmdArgument("s,source", ArgType.String, help: "Source directory", 
								  parameter_help: "source",
								  required: true,
								  assign: (dynamic d) => { srcDir = (string)d; }) },
				{ new CmdArgument("d,dest", ArgType.String, help: "Destination directory",
								  parameter_help: "dest",
								  required: true,
								  assign: (dynamic d) => { destDir = (string)d; }) },
				{ new CmdArgument("p,pattern", ArgType.String, help: "File matching pattern",
								  parameter_help: "pattern",
								  required: true,
								  assign: (dynamic d) => { pattern = (string)d; }) },
				{ new CmdArgument("m,move", ArgType.Flag, help: "Move files instead of copying",
								  assign: (dynamic d) => { moveFiles = (bool)d; }) },
				{ new CmdArgument("o,overwrite", ArgType.Flag, help: "Overwrite existing files in destination",
								  assign: (dynamic d) => { overwrite = (bool)d; }) },
				{ new CmdArgument("c,continue", ArgType.Flag, help: "Overwrite existing files in destination",
								  assign: (dynamic d) => { continueOnError = (bool)d; }) },
				{ new CmdArgument("r,preview", ArgType.Flag, help: "Preview actions only (no files copied/moved)",
								  assign: (dynamic d) => { preview = (bool)d; }) },
				{ new CmdArgument("q,quiet", ArgType.Flag, help: "Only display errors and file count",
								  assign: (dynamic d) => { quiet = (bool)d; }) }
			};

			if (!argProcessor.TryParse(args))
			{
				argProcessor.PrintHelp("FileSieve");
				return;
			}

			if (!Directory.Exists(srcDir))
			{
				Console.WriteLine("Source directory not found.");
				return;
			}
			srcDir = Path.GetFullPath(srcDir);

			if (!Directory.Exists(destDir))
			{
				Console.WriteLine("Destination directory not found.");
				return;
			}
			destDir = Path.GetFullPath(destDir);

			SieveFiles("");
		}

		static bool SieveFiles(string subPath)
		{
			string workingDir = srcDir + subPath;
			//Console.WriteLine(subPath);
			var dirList = Directory.GetDirectories(workingDir);
			foreach (var dir in dirList)
			{
				string relPath = dir.Substring(srcDir.Length);
				if (!SieveFiles(relPath))
					return false;
			}

			string destWorkingDir = destDir + subPath;
			var fileList = Directory.GetFiles(workingDir, pattern);
			foreach (var file in fileList)
			{
				try
				{
					string destFile = destWorkingDir + @"\" + Path.GetFileName(file);
                    string relativeName;
                    if (subPath.Length > 1)
					    relativeName = subPath.Substring(1) + @"\" + Path.GetFileName(file);
                    else
                        relativeName = Path.GetFileName(file);

                    if (!preview)
					{
						if (!Directory.Exists(destWorkingDir))
							Directory.CreateDirectory(destWorkingDir);

						if (File.Exists(destFile))
						{
							if (!overwrite)
							{
								if (!quiet)
									Console.WriteLine("Skipping " + TextUtils.TrimFilename(relativeName, Console.BufferWidth - 10));
								continue;
							}
							File.Delete(destFile);
						}
					}

					if (!quiet || preview)
						Console.WriteLine((moveFiles ? "Moving " : "Copying ") +
											TextUtils.TrimFilename(relativeName, Console.BufferWidth - (moveFiles ? 8 : 9)));

					if (!preview)
					{
						if (!moveFiles)
							File.Copy(file, destFile);
						else
							File.Move(file, destFile);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					return false;
				}
			}

			return true;
		}
	}
}

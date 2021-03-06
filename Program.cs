using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaDevices;
namespace mtpcopy
{
    class Program
    {
        static string GetArgument(string[] args, string[] argNames)
        {
            string argVal = "";
            for (int i = 0; i < args.Length; i++)
            {
                if (argVal.Length > 0)
                    break;
                foreach (string argName in argNames)
                {
                    if (args[i] == argName)
                        argVal = args[i+1];
                        break;
                }
            }
            return argVal;
        }
        static long WriteSreamToDisk(string filePath, MemoryStream memoryStream, bool mkTree=true)
        {
            long wroteBytes = 0;
            var dirName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirName) && mkTree)
                Directory.CreateDirectory(dirName);
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                byte[] bytes = new byte[memoryStream.Length];
                memoryStream.Read(bytes, 0, (int)memoryStream.Length);
                file.Write(bytes, 0, bytes.Length);
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists && fileInfo.Length == memoryStream.Length)
                    wroteBytes = fileInfo.Length;
                memoryStream.Close();
            }
            return wroteBytes;
        }
        static void PrintUsage()
        {
            string [] usageLines = {
                "mtpcopy usage:\n",
                "dotnet run <args 1..N>\n",
                "Arguments",
                "---------",
                "[ -d, --device ] Specify the friendly name of the MTP device (i.e. \"Apple iPhone\")",
                "[ -m, --dcim ] Specify the path of the DCIM directory on the MTP device (i.e. \"Internal Storage\\\\DCIM\")",
                "[ -o, --output ] Specify the local output path to copy files from the MTP device (i.e. \"C:\\\\iPhone\")",
                "[ -s, --search ] Specify the search filter when enumerating files on the MTP device (i.e. \"*.mov|*.jpg)\"",
            };
            foreach (var l in usageLines)
                Console.WriteLine(l);
        }
        static void Main(string[] args)
        {
            string deviceName = "";
            string dcimDirectory = "";
            string outputDirectory = "";
            string searchFilter = "";
            bool overwriteExisting = false;
            for (int i = 0; i < args.Length; i++)
            {
                deviceName = GetArgument(args, new string[]{"-d", "--device"});
                dcimDirectory = GetArgument(args, new string[]{"-m", "--dcim"});
                outputDirectory = GetArgument(args, new string[]{"-o", "--output"});
                searchFilter = GetArgument(args, new string[]{"-s", "--search"});
            }
            if (deviceName.Length == 0 || dcimDirectory.Length == 0 || outputDirectory.Length == 0)
            {
                PrintUsage();
                return;
            }
            var devices = MediaDevice.GetDevices();
            Console.WriteLine($@"Found {devices.Count()} MTP devices");
            List<string> errorFiles = new List<string>();
            using (var device = devices.First(d => d.FriendlyName == deviceName))
            {
                device.Connect();
                Console.WriteLine($@"Connected to MTP device {device.FriendlyName}");
                var dcimDir = device.GetDirectoryInfo(dcimDirectory);
                var subDirs = dcimDir.EnumerateDirectories();
                foreach (var subDir in subDirs)
                {
                    var files = subDir.EnumerateFiles(searchFilter);
                    Console.WriteLine($@"Found {files.Count()} matching filter {searchFilter} on device");                    
                    foreach (var f in files)
                    {
                        var outputPath = Path.GetFullPath($@"{outputDirectory}/{dcimDirectory}/{subDir.Name}/{f.Name}");
                        bool fileExists = File.Exists(outputPath);
                        if (fileExists && !overwriteExisting)
                        {
                            Console.WriteLine($@"File already exists: {outputPath}");
                        }
                        else if (!fileExists || (fileExists && overwriteExisting))
                        {
                            MemoryStream memoryStream = new MemoryStream();
                            try
                            {
                                device.DownloadFile(f.FullName, memoryStream);
                                memoryStream.Position = 0;
                                long wroteBytes = WriteSreamToDisk(outputPath, memoryStream);
                                if (wroteBytes == 0)
                                    Console.WriteLine($@"Error writing {outputPath} to disk!");
                                else
                                    Console.WriteLine($@"Wrote file {outputPath} ({wroteBytes} bytes) to disk");
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($@"Handled exception: {e.Message}");
                                errorFiles.Add(f.FullName);
                            }
                        }
                    }
                }
                device.Disconnect();
            }
            if (errorFiles.Count > 0)
            {
                Console.WriteLine("Handled exception when attempting to download the following files from the MTP device:");
                foreach(var errorFile in errorFiles)
                {
                    Console.WriteLine(errorFile);
                }
            }
        }
    }
}

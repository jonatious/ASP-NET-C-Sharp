using System;
using System.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using System.IO;
using System.IO.Compression;
using slnBackup_copyToProd.Classes;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;

namespace slnBackup_copyToProd.Classes
{
    class RestoreControls
    {

        public static void RestoreBackup()
        {
            Console.WriteLine("Enter backup's path in this format: X:\\YYYY\\ZZZZZ.zip");
            Constants.backup = Console.ReadLine();

            if (!File.Exists(Constants.backup))
            {
                Console.WriteLine("File not found");
                return;
            }

            Console.WriteLine("Enter destination folder's path in this format: X:\\YYYY\\ZZZZZ");
            Constants.destination = Console.ReadLine();
            Constants.destination += "\\";

            if (!Directory.Exists(Constants.destination))
            {
                Console.WriteLine("Invalid destination path!");
                return;
            }

            Restore(Constants.backup, Constants.destination);
        }

        public static void Restore(string backzip, string destination)
        {
            Constants.backup = Path.GetDirectoryName(backzip) + "\\unzip\\";
            if (Directory.Exists(Constants.backup))
            {
                try
                {
                    Directory.Delete(Constants.backup, true);
                }
                catch (Exception e) { }
            }
            Directory.CreateDirectory(Constants.backup);
            try
            {
                ZipFile.ExtractToDirectory(backzip, Constants.backup);
            }
            catch (Exception e)
            {
                Console.WriteLine("Invalid zip file!");
                return;
            }
            string temp = destination.Substring(0, destination.LastIndexOf("\\"));
            temp = temp.Substring(temp.LastIndexOf("\\") + 1, temp.Length - temp.LastIndexOf("\\") - 1);
            Constants.backup += temp + "\\";

            try
            {
                RestoreFiles(Constants.backup, destination);
            }
            catch (Exception e)
            {
                Console.WriteLine("Backup Restore Unsuccessful! Error: " + e);
                return;
            }
            Console.WriteLine("Backup Restore Successful!");
        }

        public static void RestoreFiles(string backPath, string destination)
        {
            DirectoryInfo di = new DirectoryInfo(backPath);
            IEnumerable<FileInfo> files = di.EnumerateFiles();

            foreach (FileInfo file in files)
            {
                File.Copy(file.FullName, file.FullName.Replace(Constants.backup, destination), true);
            }

            IEnumerable<DirectoryInfo> subDirs = di.EnumerateDirectories();
            foreach (DirectoryInfo sub in subDirs)
            {
                RestoreFiles(sub.FullName, destination);
            }
        }
    }
}

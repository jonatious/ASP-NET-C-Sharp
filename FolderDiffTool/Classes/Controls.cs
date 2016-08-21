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
    class Controls
    {
        public static void Compare(List<FileConstants> filesFound, List<FileConstants> destfiles_found, List<string> diffFiles)
        {
            string source_path = Constants.source;
            string dest_path = Constants.destination;

            foreach (FileConstants file in filesFound)
            {
                bool notFound = true;
                if (File.Exists((file.FilePath + "\\" + file.FileName).Replace(source_path, dest_path)))
                {
                    foreach (FileConstants destfiles in destfiles_found)
                    {
                        if (file.FileCheckSum == destfiles.FileCheckSum)
                            notFound = false;
                    }
                }

                if (notFound)
                    diffFiles.Add(file.FilePath + "\\" + file.FileName);
            }
        }

        public static void FindFiles(string source_path, List<FileConstants> filesFound)
        {
            DirectoryInfo di = new DirectoryInfo(source_path);
            IEnumerable<FileInfo> files = di.EnumerateFiles();

            foreach (FileInfo file in files)
            {
                string temp = file.Name;
                FileConstants newfile = new FileConstants(file.Name, file.Directory.FullName);
                if (!Constants.skip_list.Contains(temp))
                    filesFound.Add(newfile);
            }

            IEnumerable<DirectoryInfo> subDirs = di.EnumerateDirectories();
            foreach (DirectoryInfo sub in subDirs)
            {
                string temp = sub.FullName;
                //.Replace(Constants.source, "");
                if (temp.Contains(Constants.source))
                    temp = temp.Replace(Constants.source, "");
                else
                    temp = temp.Replace(Constants.destination, "");
                if (!Constants.skip_list.Contains(temp))
                FindFiles(sub.FullName, filesFound);
            }

        }
        public static void Print(List<string> files)
        {
            foreach (String f in files)
            {
                Console.WriteLine(f);
            }
        }

        public static void WriteResults(List<FileConstants> filesFound, List<FileConstants> destfiles_found, List<string> diffFiles)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Constants.backup log generated on " + DateTime.Now.ToString());
            sb.AppendLine();
            sb.AppendFormat("No of files found in Source path (" + Constants.source.Replace("\\\\", "\\") + ") is " + filesFound.Count);
            sb.AppendLine();
            sb.AppendFormat("No of files found in Destination path (" + Constants.destination.Replace("\\\\", "\\") + ") is " + destfiles_found.Count);
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendFormat("The following are the files that dont match between source and destination folders:");
            sb.AppendLine();

            Constants.log = sb.ToString();

            List<string> temp = new List<string>();
            temp.Add(Constants.log);

            foreach (string file in diffFiles)
            {
                temp.Add(file);
                Constants.log += file + "\n";
            }
            if (File.Exists(Constants.results))
            {
                File.Delete(Constants.results);
            }

            File.AppendAllLines(Constants.results, temp);
        }

        public static SecureString GetPassword()
        {
            SecureString pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }

        public static void GetSkiplist()
        {

        SkipList:
            Console.WriteLine("Do you have a skip folders list? y/n");
            string option = Console.ReadLine();

            switch (option)
            {
                case "y":
                    Console.WriteLine("Enter the skip list file path");
                    string skipFile = Console.ReadLine();
                    if (File.Exists(skipFile))
                        Constants.skip_list = File.ReadAllLines(skipFile);
                    break;
                case "n":
                    break;
                default:
                    Console.WriteLine("Invalid option Try Again!!\n");
                    goto SkipList;
            }
        }
    }
}

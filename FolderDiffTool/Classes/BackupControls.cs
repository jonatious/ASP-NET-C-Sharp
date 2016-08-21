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
    class BackupControls
    {
        public static void BackUp()
        {
            string option;
            Source:
            Console.WriteLine("Enter Source directory path in this format: X:\\YYYY\\ZZZZZ");
            Constants.source = Console.ReadLine();
            Constants.source += "\\";
            if (!Directory.Exists(Constants.source))
            {
                Console.WriteLine("Invalid Source path. Please try again.");
                goto Source;
            }

            Destination:
            Console.WriteLine("Enter Destination directory path in this format: X:\\YYYY\\ZZZZZ");
            Constants.destination = Console.ReadLine();
            Constants.destination += "\\";
            if (!Directory.Exists(Constants.destination))
            {
                Console.WriteLine("Invalid Destination path. Please try again.");
                goto Destination;
            }

            Backup: 
            Console.WriteLine("Enter Backup directory path in this format: X:\\YYYY\\ZZZZZ");
            Constants.backup = Console.ReadLine();
            Constants.backup += "\\";
            if (!Directory.Exists(Constants.backup))
            {
                Console.WriteLine("Invalid Backup path. Please try again.");
                goto Backup;
            }
            Constants.results = Constants.backup + "results.log";

            Controls.GetSkiplist();
            List<FileConstants> filesFound = new List<FileConstants>();
            List<FileConstants> destfilesFound = new List<FileConstants>();

            string source_path = Constants.source;
            string dest_path = Constants.destination;

            try
            {
                Controls.FindFiles(source_path, filesFound);
                Controls.FindFiles(dest_path, destfilesFound);

                //checksum times in source path
                int time = 0;
                foreach (FileConstants file in filesFound)
                {
                    time += file.FileCheckSumTime;
                }
                Console.WriteLine("Total time taken for calculating checksums for SOURCE files is " + Convert.ToDouble(time) / 1000.0 + " seconds.");

                //checksum times in destination path
                time = 0;
                foreach (FileConstants file in destfilesFound)
                {
                    time += file.FileCheckSumTime;
                }
                Console.WriteLine("Total time taken for calculating checksums for DESTINATION files is " + Convert.ToDouble(time) / 1000.0 + " seconds.");

                BackPush(filesFound, destfilesFound);

            JsonCreate:
                Console.WriteLine("\nDo you want to create json files for the source and destination folders? y/n");
                option = Console.ReadLine();

                switch (option)
                {
                    case "y":
                        JsonControls.Jsoncreate(filesFound, destfilesFound);
                        Console.WriteLine("Json file created in Backup folder (" + Constants.backup + ")\n");
                        break;
                    case "n":
                        Console.WriteLine("Json files not created!!\n");
                        break;
                    default:
                        Console.WriteLine("Invalid option Try Again!!\n");
                        goto JsonCreate;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }

        public static void BackupFiles(List<string> filesFound, List<FileConstants> sourceFiles, List<FileConstants> destFiles)
        {
            string backup_path = Constants.backup;
            string folder = DateTime.Now.ToString("yyyy_MMMM_dd");
            int i = 1;
            while (Directory.Exists(Constants.backup + folder + "_" + i))
                i++;
            folder += "_" + i;
            Constants.backup += folder;
            Directory.CreateDirectory(Constants.backup);

            //Copy log file
            File.Copy(Constants.results, Constants.backup + "\\results.log", true);

            foreach (string file in filesFound)
            {
                string temp_file = file.Replace(Constants.source, Constants.destination);
                string temp = Constants.destination.Substring(0, Constants.destination.LastIndexOf("\\"));
                string dest_path = Constants.backup + temp_file.Replace(temp.Substring(0, (temp.LastIndexOf("\\"))), "");
                //Console.WriteLine(Path.GetDirectoryName(dest_path));
                if (!Directory.Exists(Path.GetDirectoryName(dest_path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(dest_path));
                if (File.Exists(temp_file))
                    File.Copy(temp_file, dest_path, true);
            }
            Constants.backup_y_n = true;
            JsonControls.Jsoncreate(sourceFiles, destFiles);
            ZipFile.CreateFromDirectory(Constants.backup, backup_path + "\\" + folder + ".zip");
            Directory.Delete(Constants.backup, true);
            Directory.CreateDirectory(Constants.backup);
            File.Move(backup_path + "\\" + folder + ".zip", Constants.backup + "\\" + folder + ".zip");
        }

        public static void BackPush(List<FileConstants> filesFound, List<FileConstants> destfilesFound)
        {
            string option;
            List<string> diff_files = new List<string>();
            Controls.Compare(filesFound, destfilesFound, diff_files);

            if (diff_files.Count > 0)
            {
                Controls.WriteResults(filesFound, destfilesFound, diff_files);
                Console.WriteLine("Log file generated: " + Constants.results.Replace("\\\\", "\\") + "\n");
                Console.WriteLine("------------------------------ Results.log ------------------------------------");
                Console.WriteLine(Constants.log);

            Backup:
                Console.WriteLine("\nDo you want to backup these files to the backup folder(" + Constants.backup + ")? y/n");
                option = Console.ReadLine();

                switch (option)
                {
                    case "y":
                        if (Constants.choice == 2)
                        {
                            Console.WriteLine("You need to enter source and destination directories to proceed!");
                            Console.WriteLine("Enter source directory path in this format: X:\\YYYY\\ZZZZZ");
                            Constants.source = Console.ReadLine();
                            Constants.source += "\\";

                            Console.WriteLine("Enter destination directory path in this format: X:\\YYYY\\ZZZZZ");
                            Constants.destination = Console.ReadLine();
                            Constants.destination += "\\";
                            Constants.choice = 1;
                        }
                        try
                        {
                            BackupFiles(diff_files, filesFound, destfilesFound);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error: " + e);
                            Console.WriteLine("Backup UnSuccessful!!\n");
                            return;
                        }
                        Console.WriteLine("Backup Successful!!\n");
                        break;
                    case "n":
                        Console.WriteLine("Backup was not taken!!\n");
                        break;
                    default:
                        Console.WriteLine("Invalid option Try Again!!\n");
                        goto Backup;
                }

            PushtoDest:
                Console.WriteLine("\nDo you want to push these files to the destination folder(" + Constants.destination + ")? y/n");
                option = Console.ReadLine();

                switch (option)
                {
                    case "y":
                        if (Constants.choice == 2)
                        {
                            Console.WriteLine("You need to enter source and destination directories to proceed!");
                            Console.WriteLine("Enter source directory path in this format: X:\\YYYY\\ZZZZZ");
                            Constants.source = Console.ReadLine();
                            Constants.source += "\\";

                            Console.WriteLine("Enter destination directory path in this format: X:\\YYYY\\ZZZZZ");
                            Constants.destination = Console.ReadLine();
                            Constants.destination += "\\";
                            Constants.choice = 1;
                        }
                        PushFiles(diff_files);
                        Console.WriteLine("Push to Destination Successful!!\n");
                        break;
                    case "n":
                        Console.WriteLine("Files were not pushed to Destination directory!!\n");
                        break;
                    default:
                        Console.WriteLine("Invalid option Try Again!!\n");
                        goto PushtoDest;
                }
            }
            else
                Console.WriteLine("All files in source folder are already present in destination folder and there are no changes!!");

        }

        public static void PushFiles(List<string> filesFound)
        {

            foreach (string file in filesFound)
            {
                string dest_file = file.Replace(Constants.source, Constants.destination);
                if (!Directory.Exists(Path.GetDirectoryName(dest_file)))
                    Directory.CreateDirectory(Path.GetDirectoryName(dest_file));
                if (File.Exists(dest_file))
                    File.Delete(dest_file);
                File.Copy(file, dest_file, true);
            }

            Console.WriteLine("Do you have other servers to push to? y/n");
            string option = Console.ReadLine();
            if (option.Equals("y"))
                ServerPush(filesFound);
            Console.WriteLine("Push to destination successful!");
        }

        public static void ServerPush(List<string> filesFound)
        {
            string server;
        //SecureString password;
        ServerStart:
            Console.Write("\nShared folder path: ");
            server = Console.ReadLine();
            server = server + "\\";
            //if(Directory.Exists(server))
            /*Console.Write("Domain: ");
            domain = Console.ReadLine();
            Console.Write("Username: ");
            username = Console.ReadLine();
            Console.Write("Password: ");
            password = GetPassword();*/

            try
            {
                foreach (string file in filesFound)
                {
                    string dest_file = file.Replace(Constants.source, server);
                    if (!Directory.Exists(Path.GetDirectoryName(dest_file)))
                        Directory.CreateDirectory(Path.GetDirectoryName(dest_file));
                    if (File.Exists(dest_file))
                        File.Delete(dest_file);
                    File.Copy(file, dest_file, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n\nInvalid Server or Credentials!\nError: " + e);
                goto ServerStart;
            }

            Console.WriteLine("Do you have other servers to push to? y/n");
            string option = Console.ReadLine();
            if (option.Equals("y"))
                ServerPush(filesFound);
        }
    }
}

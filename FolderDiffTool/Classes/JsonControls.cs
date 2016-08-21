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
    class JsonControls
    {
        public static void Jsoncreate(List<FileConstants> sourceFiles, List<FileConstants> destFiles)
        {
            if (!Constants.backup_y_n)
            {
                string folder = DateTime.Now.ToString("yyyy_MMMM_dd");
                int i = 1;
                while (Directory.Exists(Constants.backup + folder + "_" + i))
                    i++;
                folder += "_" + i;
                Constants.backup += folder;
                Directory.CreateDirectory(Constants.backup);

                //Copy log file
                File.Copy(Constants.results, Constants.backup + "\\results.log", true);
            }
            //Creating json for source folder
            jsonObject temp = new jsonObject();
            temp.TimeCreated = DateTime.Now;
            temp.Path = Constants.source;
            temp.FileData = JsonConvert.SerializeObject(sourceFiles);
            string tempJson = JsonConvert.SerializeObject(temp);
            string fileName = Constants.backup + "\\source.json";
            File.WriteAllText(fileName, tempJson);

            //Creating json for destination folder
            temp.TimeCreated = DateTime.Now;
            temp.Path = Constants.destination;
            temp.FileData = JsonConvert.SerializeObject(destFiles);
            tempJson = JsonConvert.SerializeObject(temp);
            fileName = Constants.backup + "\\destination.json";
            File.WriteAllText(fileName, tempJson);
        }

        public static void JsonCompare()
        {
            Console.WriteLine("Enter source folder's json path in this format: X:\\YYYY\\ZZZZZ.json");
            Constants.source = Console.ReadLine();

            Console.WriteLine("Enter destination folder's json path in this format: X:\\YYYY\\ZZZZZ.json");
            Constants.destination = Console.ReadLine();

            Console.WriteLine("Enter backup directory path in this format: X:\\YYYY\\ZZZZZ");
            Constants.backup = Console.ReadLine();
            Constants.backup += "\\";
            Constants.results = Constants.backup + "results.log";
            Controls.GetSkiplist();

            List<FileConstants> filesFound = new List<FileConstants>();
            List<FileConstants> destfilesFound = new List<FileConstants>();

            //Reading source folder's info from Json
            string jsonInput = File.ReadAllText(Constants.source);
            jsonObject tempJson = JsonConvert.DeserializeObject<jsonObject>(jsonInput);
            List<FileTempConsts> tempFiles = new List<FileTempConsts>();
            tempFiles = JsonConvert.DeserializeObject<List<FileTempConsts>>(tempJson.FileData);
            foreach (FileTempConsts t in tempFiles)
            {
                FileConstants temp = new FileConstants();
                temp.AddData(t.FileName, t.FilePath, t.FileCheckSum, t.FileCheckSumTime);
                filesFound.Add(temp);
            }

            //Reading destination folder's info from Json
            jsonInput = File.ReadAllText(Constants.destination);
            jsonObject tempJson1 = JsonConvert.DeserializeObject<jsonObject>(jsonInput);
            tempFiles = JsonConvert.DeserializeObject<List<FileTempConsts>>(tempJson1.FileData);
            foreach (FileTempConsts t in tempFiles)
            {
                FileConstants temp = new FileConstants();
                temp.AddData(t.FileName, t.FilePath, t.FileCheckSum, t.FileCheckSumTime);
                destfilesFound.Add(temp);
            }

            BackupControls.BackPush(filesFound, destfilesFound);
        }
    }
}

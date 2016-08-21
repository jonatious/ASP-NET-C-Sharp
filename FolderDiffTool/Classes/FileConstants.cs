using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace slnBackup_copyToProd.Classes
{
    public class FileConstants
    {
        #region MEMBERS
        // session keys
        public string _file_name;
        public string _file_path;
        public string _checksum;
        public int _timeTaken;

        #endregion

        #region PROPERTIES
        public FileConstants()
        { }
        public FileConstants(string File_name, string File_path)
        {
            _file_name = File_name;
            _file_path = File_path;
            _checksum = computeChecksum();
        }
        
        public void AddData(string File_name, string File_path, string checksum, int time)
        {
            _file_name = File_name;
            _file_path = File_path;
            _checksum = checksum;
            _timeTaken = time;
        }

        public string computeChecksum()
        {
            string checksum = string.Empty;
            int start = Environment.TickCount;
            using (FileStream stream = File.OpenRead(_file_path + "\\" + _file_name))
            {
                SHA256Managed sha = new SHA256Managed();
                byte[] hash = sha.ComputeHash(stream);
                checksum = BitConverter.ToString(hash).Replace("-", String.Empty);
            }
            int end = Environment.TickCount;

            _timeTaken = end - start;
            return checksum;
        }

        public string FileName
        {
            get { return _file_name; }
            set { _file_name = value; }
        }

        public string FilePath
        {
            get
            {
                return _file_path;
            }
            set { _file_path = value; }
        }

        public string FileCheckSum
        {
            get
            {
                return _checksum;
            }
            set { _checksum = value; }
        }

        public int FileCheckSumTime
        {
            get
            {
                return _timeTaken;
            }
            set { _timeTaken = value; }
        }

        #endregion
    }
}
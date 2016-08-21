using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace slnBackup_copyToProd.Classes
{
    public class Constants
    {
        
        public static string source;
        public static string destination;
        public static string backup;
        public static string results;
        public static string log;
        public static string[] skip_list = new String[] { "" };
        public static int choice = 0;
        public static bool backup_y_n = false;
    }
}
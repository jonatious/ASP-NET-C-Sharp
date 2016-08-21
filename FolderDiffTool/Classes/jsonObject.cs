using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slnBackup_copyToProd.Classes
{
    class jsonObject
    {
        public DateTime TimeCreated { get; set; }
        public string Path { get; set; }
        public string FileData { get; set; }
    }
}

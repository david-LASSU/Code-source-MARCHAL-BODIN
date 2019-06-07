using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBCore.Model
{
    public class Database
    {
        public int id { get; set; }
        public string name { get; set; }
        public string server { get; set; }
        public string sageDir { get; set; }
        public string email { get; set; }
        // Base magasin?
        public bool isMag { get; set; }
        public string context => name.Substring(name.Length - 3) == "DEV" ? "DEV" : "PROD";
        public string originalName => name.Replace("DEV", "");
        public string gcmFile => $"{sageDir}\\{name}.gcm";
        public string maeFile => $"{sageDir}\\{name}.mae";
        public string cnxString => $"server={server};Trusted_Connection=True;Database={name};MultipleActiveResultSets=True";
        // Raccourcis vers [hostname].[database].[dbo]
        public string dboChain => $"[{server}].[{name}].[dbo]";
        public override string ToString()
        {
            return name;
        }
    }
}

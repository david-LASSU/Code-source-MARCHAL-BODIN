using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDb.Model
{
    public class Ligne
    {
        public string ObjectName { get; set; }
        public string PKValue { get; set; }
        public bool IsInsert { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MBCore.Model
{
    public class Unite
    {
        public int CbMarq { get; set; }
        public string Intitule { get; set; }
        public override string ToString()
        {
            return Intitule;
        }
    }
}

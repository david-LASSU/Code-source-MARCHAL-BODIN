using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBCore.Model
{
    public class Fournisseur
    {
        public string CtNum { get; set; }
        public string Intitule { get; set; }
        public string ConcatName => $"{CtNum} - {Intitule}";
        public override string ToString()
        {
            return ConcatName;
        }
    }
}

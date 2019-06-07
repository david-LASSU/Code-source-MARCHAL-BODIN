using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElmUtilisation.Model
{
    class LockLog
    {
        public int Callid { get; set; }
        public DateTime CallTime { get; set; }
        public string CallingUser { get; set; }
        public string CallingMachine { get; set; }
        public string CbFile { get; set; }
        public int CbMarq { get; set; }
        public string Ref { get; set; }

        public string Objet
        {
            get
            {
                switch (CbFile)
                {
                    case "F_FAMILLE":
                        return "Famille";
                    case "F_ARTICLE":
                        return "Article";
                    case "F_COMPTET":
                        return "Tiers";
                    case "F_DOCENTETE":
                        return "Document";
                    case "F_COMPTEG":
                        return "Compte général";
                    case "F_JMOUV":
                        return "Mouvement";
                    case "F_CAISSE":
                        return "Caisse";
                    case "F_CONTACTT":
                        return "Contact Tiers";
                    default:
                        return CbFile;
                }
            }
        }
    }
}

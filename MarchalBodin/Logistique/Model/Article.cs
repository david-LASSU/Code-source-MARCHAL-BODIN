using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logistique.Model
{
    public class Article
    {
        public string Ref => AeRef != null ? AeRef : ArRef;
        public string Designation { get; set; }
        public double Stock { get; set; }
        public string ArRef { get; set; }
        public string AeRef { get; internal set; }
        public string Gamme1 { get; internal set; }
        public string Gamme2 { get; internal set; }
    }
}

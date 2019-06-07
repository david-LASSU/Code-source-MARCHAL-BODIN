using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MBCore.Model
{
    public class User
    {
        public string cbSession { get; set; }
        public string sessionId { get; set; }
        public string hostProcessId { get; set; }
        public string programName { get; set; }
        public string CbType { get; set; }
        public string cbUserName { get; set; }
        public string hostname { get; set; }
        public string ntUsername { get; set; }
    }
}

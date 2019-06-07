using InterMagService.Model;
using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IntermagServiceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var repos = new Repository();
            repos.UpdateDocuments();
            Console.WriteLine("Fin ...");
            Console.ReadKey();
        }
    }
}

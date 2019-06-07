using MajFournDNA;
using MBCore.Model;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MajFournDNATest
{
    class Program
    {
        static void Main(string[] args)
        {
            var processes = from p in Process.GetProcessesByName("EXCEL") select p;
            foreach (var process in processes)
            {
                process.Kill();
            }
            Application ExcelApp = new Application();
            Workbook book = ExcelApp.Workbooks.Open("S:\\COMPTA\\MAJ FOURN\\EN COURS FRED\\Pour MANU\\Index EAN Condi.xlsm");
            Worksheet sheet = book.Worksheets["Articles"];
            Worksheet parametres = book.Worksheets["parametres"];
            ListObject tableArticle = sheet.ListObjects["TableArticles"];

            //BaseCialAbstract.setDefaultParams("G:\\_SOCIETES\\TARIF.gcm");
            //BaseCialAbstract.setDefaultParams("G:\\_SOCIETES\\DEV\\TARIFDEV.gcm");
            Repository repos = new Repository();

            string res;
            foreach (ListRow row in tableArticle.ListRows)
            {
                res = repos.EnregistrerArticle(row, parametres.Range["fournId"].Value);
                Debug.Print(res);
            }
            book.Close(false);
            
            Console.WriteLine("FIN");
            Console.ReadKey();
        }
    }
}

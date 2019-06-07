using ExcelDna.ComInterop;
using ExcelDna.Integration;
using MBCore.Model;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MajFournDNA
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [ProgId("MajFourn_functionLibrary")]
    public class Functions
    {
        public string EnregistrerArticle(ListRow row, string fichier, string ctNum)
        {
            BaseCialAbstract.setDefaultParams(fichier);
            Repository repos = new Repository();
            BaseCialAbstract.setDefaultParams(repos.getDbListFromContext().First(db => db.name.Contains("TARIF")).gcmFile);

            return repos.EnregistrerArticle(row, ctNum);
        }

        public string GetBaseSelect()
        {
            return Repository.GetBaseSelect();
        }
    }
    //
    [ComVisible(false)]
    class ExcelAddin : IExcelAddIn
    {
        public void AutoOpen()
        {
            ComServer.DllRegisterServer();
        }
        public void AutoClose()
        {
            ComServer.DllUnregisterServer();
        }
    }
}

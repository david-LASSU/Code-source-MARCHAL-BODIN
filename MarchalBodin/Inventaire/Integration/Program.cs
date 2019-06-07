using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Data.SqlClient;
using System.IO;

namespace Integration
{
    class Program
    {
        private static string server = "SRVSQL01.MARCHAL-BODIN.LOCAL";
        private static string database = "SMO";
        private static string cnxString = "server={0};Trusted_Connection=yes;database={1};MultipleActiveResultSets=True";
        private static string dir = "R:\\INVENTAIRE\\2016-2017\\";
        private static string fichierCumul = dir + "CUMUL.xlsx";
        private static string fichierSage = dir + "inventaire.txt";
        private static string fichierLog = dir + "log.log";
        private static DateTime dateIntegration = new DateTime(2017, 5, 31);

        private const int COLEMPL = 1;
        private const int COLREF = 2;
        private const int COLQT = 5;
        private const int COLEAN = 6;
        private const int COLPXACH = 9;
        private const int COLDESIGN = 8;
        private const int COLDATE = 19;

        /**
         * 0 -> AR_Ref
         * 1 -> Stock
         * 2 -> AR_PrixAch
         * 3 -> DP_Code
         **/
        private static string strLine = "{0}				{1}	{2}				{3}																																																																";
        private static Application ExcelApp;

        static void Main(string[] args)
        {
            // Vide le log
            File.WriteAllText(fichierLog, String.Empty);
            cnxString = string.Format(cnxString, server, database);

            // Kill Excel
            var processes = from p in Process.GetProcessesByName("EXCEL") select p;
            foreach (var process in processes)
            {
                process.Kill();
            }
            ExcelApp = new Application();

            // Cumul, uniquement Sobrigir pour le moment
            //cumul();

            exportFichier();

            chercheNoninventories();

            log("FIN");
        }

        static void cumul()
        {
            Workbook cumulBook = ExcelApp.Workbooks.Open(fichierCumul);
            Worksheet sheetSaisi = cumulBook.Sheets["Saisi"];
            sheetSaisi.UsedRange.Clear();
            Dictionary<string, LigneInventaire> dicoRefs = new Dictionary<string, LigneInventaire>();
            List<LigneInventaire> listeI = new List<LigneInventaire>();

            string refMag, designation, empl, ean;
            double qt, pxAch;
            DateTime date;
            LigneInventaire ligne;

            foreach (string file in Directory.GetFiles(dir, "*.xlsm", SearchOption.AllDirectories))
            {
                log(file);
                Workbook book = ExcelApp.Workbooks.Open(file);
                Worksheet sheet = book.Worksheets["INVENTAIRE"];

                int rc = sheet.UsedRange.Count;
                try
                {
                    for (int i = 4; i < rc; i++)
                    {
                        refMag = sheet.Cells[i, COLREF].Value().ToString();
                        refMag = refMag.Trim().ToUpper();

                        if (IsXLCVErr(sheet.Cells[i, COLREF].Value()))
                        {
                            refMag = "I";
                        }

                        // Fin du fichier
                        if (refMag == null || refMag == "") { break; }

                        //empl = "COMPTOIR";

                        empl = sheet.Cells[i, COLEMPL].Value();
                        if (empl == null)
                        {
                            empl = "COMPTOIR";
                        }

                        empl = empl.ToUpper();

                        qt = sheet.Cells[i, COLQT].Value() == null ? 1 : double.Parse(sheet.Cells[i, COLQT].Value().ToString());
                        date = sheet.Cells[i, COLDATE].Value() ?? new DateTime();
                        pxAch = (sheet.Cells[i, COLPXACH].Value() == null || sheet.Cells[i, COLPXACH].Value() < 0) ? 0 : sheet.Cells[i, COLPXACH].Value();
                        designation = sheet.Cells[i, COLDESIGN].Value().ToString();

                        try
                        {
                            ean = sheet.Cells[i, COLEAN].Value()?.ToString("0");
                        }
                        catch (Exception) { ean = null; }

                        log($"Ligne {i} :: {empl} :: {refMag} :: {ean} :: {qt} :: {date.ToShortDateString()}");

                        ligne = new LigneInventaire() {
                            emplacement = empl,
                            reference = refMag,
                            codeBarre = ean,
                            quantite = qt,
                            date = date,
                            designation = designation,
                            prixAch = pxAch,
                        };

                        // I
                        if (refMag == "I")
                        {
                            listeI.Add( ligne);
                            continue;
                        }
                        
                        // Vrai ref
                        if (dicoRefs.ContainsKey(refMag))
                        {
                            dicoRefs[refMag].quantite += qt;
                            dicoRefs[refMag].date = date;
                        }
                        else
                        {
                            dicoRefs.Add(refMag, ligne);
                        }
                        dicoRefs[refMag].total = dicoRefs[refMag].quantite * dicoRefs[refMag].prixAch;
                    }
                }
                catch (Exception ex)
                {
                    log(ex.ToString());
                }

                book.Close(false);
            }

            int il = 2;
            foreach (LigneInventaire l in dicoRefs.Values)
            {
                try
                {
                    sheetSaisi.Cells[il, 1] = l.emplacement;
                    sheetSaisi.Cells[il, 2] = l.reference;
                    sheetSaisi.Cells[il, 3] = l.codeBarre;
                    sheetSaisi.Cells[il, 4] = l.quantite;
                    sheetSaisi.Cells[il, 5] = l.prixAch;
                    sheetSaisi.Cells[il, 6] = l.total;
                    sheetSaisi.Cells[il, 7] = l.designation;
                    sheetSaisi.Cells[il, 8] = l.date;
                }
                catch (Exception ex)
                {
                    log($"ERREUR Ligne {il} : {ex.Message}");
                }

                il++;
            }

            foreach (LigneInventaire l in listeI)
            {
                try
                {
                    sheetSaisi.Cells[il, 1] = l.emplacement;
                    sheetSaisi.Cells[il, 2] = l.reference;
                    sheetSaisi.Cells[il, 3] = l.codeBarre;
                    sheetSaisi.Cells[il, 4] = l.quantite;
                    sheetSaisi.Cells[il, 5] = l.prixAch;
                    sheetSaisi.Cells[il, 6] = l.total;
                    sheetSaisi.Cells[il, 7] = l.designation;
                    sheetSaisi.Cells[il, 8] = l.date;
                }
                catch (Exception ex)
                {
                    log($"ERREUR Ligne {il} : {ex.Message}");
                }

                il++;
            }

            sheetSaisi.EnableAutoFilter = true;
            Range firstRow = sheetSaisi.Rows[1];
            firstRow.Cells[1].Value = "Emplacement";
            firstRow.Cells[2].Value = "REF";
            firstRow.Cells[3].Value = "Code Barre";
            firstRow.Cells[4].Value = "Quantité";
            firstRow.Cells[5].Value = "Prix Achat";
            firstRow.Cells[6].Value = "Total";
            firstRow.Cells[7].Value = "Désignation";
            firstRow.Cells[8].Value = "Date";
            firstRow.AutoFilter(1);
            sheetSaisi.Columns.AutoFit();

            cumulBook.Close(true);
        }

        static void exportFichier()
        {
            // Vide le fichier d'inventaire
            File.WriteAllText(fichierSage, "");

            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                log(fichierCumul);

                Workbook book = ExcelApp.Workbooks.Open(fichierCumul);
                Worksheet sheetSaisi = book.Sheets["Saisi"];
                Worksheet sheetRecap = book.Sheets["Recap"];
                // Non trouvés dans Sage
                Worksheet sheetNTSage = book.Sheets["NonTrouveSage"];
                sheetRecap.UsedRange.Clear();
                sheetNTSage.UsedRange.Clear();

                int rc = sheetSaisi.UsedRange.Count;
                try
                {
                    int xlI = 2;
                    int xlI2 = 2;
                    for (int i = 2; i < rc; i++)
                    {
                        string empl = sheetSaisi.Cells[i, 1].Value();
                        string refMag = sheetSaisi.Cells[i, 2].Value();
                        
                        // Fin du fichier
                        if (refMag == null) { break; }

                        refMag = refMag.ToUpper();

                        double qt = sheetSaisi.Cells[i, 3].Value();
                        DateTime date = sheetSaisi.Cells[i, 17].Value() ?? new DateTime();

                        //if (date.ToShortDateString() != "18/05/2016") { continue; }
                        if (refMag.ToLower() == "i") { continue; }
                        string uniteAchat = sheetSaisi.Cells[i, 10].Value();
                        string uniteVente = sheetSaisi.Cells[i, 11].Value();

                        log($"Ligne {i} :: {empl} :: {refMag} :: {qt} :: {date.ToShortDateString()}");

                        // L'article a été vendu ou acheté ?
                        using (SqlCommand cmd = cnx.CreateCommand())
                        {
                            cmd.CommandText = @"SELECT DO_Domaine, DO_Type, DL.AR_Ref, A.AR_Design, DateMvt, DL.DL_Qte, DEPO.DP_Code
                                FROM F_DOCLIGNE DL
                                JOIN F_ARTICLE A ON A.AR_Ref = DL.AR_Ref
                                LEFT JOIN F_ARTSTOCK AS ARTS ON A.AR_Ref = ARTS.AR_ref
                                LEFT JOIN F_DEPOTEMPL AS DEPO on ARTS.DP_NoPrincipal = DEPO.DP_No
                                WHERE DateMvt IS NOT NULL 
                                    -- AND A.AR_SuiviStock <> 0
                                    AND A.AR_Ref = @arRef
                                    AND DateMvt > CONVERT(datetime, @dateTime, 121)
                                    AND DateMvt < CONVERT(datetime, @dateIntegration, 121)";

                            /// !!!! ATTENTION ne pas inclure les factures d'avoir

                            cmd.Parameters.AddWithValue("@arRef", refMag);
                            cmd.Parameters.AddWithValue("@dateTime", date.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.Parameters.AddWithValue("@dateIntegration", dateIntegration.ToString("yyyy-MM-dd HH:mm:ss"));

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    double dlQte;
                                    DateTime mvtDate;
                                    while (reader.Read())
                                    {
                                        dlQte = decimal.ToDouble((decimal)reader["DL_Qte"]);
                                        mvtDate = (DateTime)reader["DateMvt"];
                                        switch ((short)reader["DO_Domaine"])
                                        {
                                            case 0:
                                                qt -= dlQte;
                                                log($"- {dlQte} = {qt} le {mvtDate.ToShortDateString()} à {mvtDate.ToLongTimeString()}");
                                                break;
                                            case 1:
                                                qt += dlQte;
                                                log($"+ {dlQte} = {qt} le {mvtDate.ToShortDateString()} à {mvtDate.ToLongTimeString()}");
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        // TODO prendre le prix d'achat sur F_ARTICLE::AR_PxAchat
                        using (SqlCommand cmd = cnx.CreateCommand())
                        {
                            cmd.CommandText = @"SELECT A.AR_Design, A.AR_PrixAch, AF.AF_RefFourniss 
                                                FROM F_ARTICLE A LEFT JOIN F_ARTFOURNISS AF ON A.AR_Ref = AF.AR_Ref AND AF_Principal = 1 
                                                WHERE A.AR_Ref = @arRef ";
                            cmd.Parameters.AddWithValue("@arRef", refMag);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    reader.Read();

                                    string design = (string)reader["AR_Design"];
                                    string refFourn = reader["AF_RefFourniss"].ToString();
                                    decimal pxAch = (decimal)reader["AR_PrixAch"];

                                    // Si /$ on recup le prix d'achat de la centaine et on modifie la feuille recap
                                    if (refFourn.Length > 2 && refFourn.Substring(0, 2).Contains("/$"))
                                    {
                                        string refCent = refMag.Split('-').First() + '-' + (Int32.Parse(refMag.Split('-').Last()) - 1).ToString().PadLeft(4, '0');
                                        using (SqlCommand cmdCond = cnx.CreateCommand())
                                        {
                                            cmdCond.CommandText = @"SELECT A.AR_Design, AF.AF_PrixAch
                                                                    FROM F_ARTICLE A
                                                                    JOIN F_ARTFOURNISS AF ON A.AR_Ref = AF.AR_Ref AND AF.AF_Principal = 1
                                                                    WHERE A.AR_Ref = @refCent";
                                            cmdCond.Parameters.AddWithValue("@refCent", refCent);

                                            using (SqlDataReader rCond = cmdCond.ExecuteReader())
                                            {
                                                if (rCond.HasRows)
                                                {
                                                    rCond.Read();
                                                    log("REF PIECE /$");
                                                    pxAch = (decimal)rCond["AF_PrixAch"];
                                                    design = (string)rCond["AR_Design"];
                                                    using (StreamWriter w = File.AppendText(fichierSage))
                                                    {
                                                        // Passe la qt de la ref Pièce à 0 dans le fichier inventaire
                                                        w.WriteLine(strLine, refMag, 0, pxAch.ToString().Replace(".", ","), empl);
                                                        // Set l'emplacement par defaut de la ref pièce
                                                        setDefaultEmpl(cnx, refMag, empl);

                                                        // Inventorie le stock sur la ref à la centaine
                                                        refMag = refCent.ToUpper();
                                                        qt = qt / 100;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (uniteAchat == "CENT" && uniteVente == "CENT")
                                    {
                                        log("REF CENTAINE");
                                        qt = qt / 100;
                                    }

                                    using (StreamWriter w = File.AppendText(fichierSage))
                                    {
                                        w.WriteLine(strLine, refMag, qt.ToString().Replace(".", ","), pxAch.ToString().Replace(".", ","), empl);
                                    }

                                    sheetRecap.Cells[xlI, 1] = empl;
                                    sheetRecap.Cells[xlI, 2] = refMag;
                                    sheetRecap.Cells[xlI, 3] = qt;
                                    sheetRecap.Cells[xlI, 4] = pxAch;
                                    sheetRecap.Cells[xlI, 5] = qt * sheetRecap.Cells[xlI, 4].Value;
                                    sheetRecap.Cells[xlI, 6] = design;
                                    xlI++;

                                    // Set l'emplacement par defaut
                                    setDefaultEmpl(cnx, refMag, empl);
                                }
                                else
                                {
                                    log("REF NON TROUVE");

                                    sheetNTSage.Cells[xlI2, 1] = empl;
                                    sheetNTSage.Cells[xlI2, 2] = refMag;
                                    sheetNTSage.Cells[xlI2, 3] = qt;
                                    sheetNTSage.Cells[xlI2, 4] = sheetSaisi.Cells[i, 4];
                                    sheetNTSage.Cells[xlI2, 5] = sheetSaisi.Cells[i, 5];
                                    sheetNTSage.Cells[xlI2, 6] = sheetSaisi.Cells[i, 6];
                                    sheetNTSage.Cells[xlI2, 7] = "REF NON TROUVE";
                                    xlI2++;
                                }
                            }
                        }
                    }

                    sheetRecap.EnableAutoFilter = true;
                    Range firstRow = sheetRecap.Rows[1];
                    firstRow.Cells[1].Value = "Emplacement";
                    firstRow.Cells[2].Value = "REF";
                    firstRow.Cells[3].Value = "Quantité";
                    firstRow.Cells[4].Value = "Prix Achat";
                    firstRow.Cells[5].Value = "Total";
                    firstRow.Cells[6].Value = "Désignation";
                    firstRow.AutoFilter(1);
                    sheetRecap.Columns.AutoFit();

                    firstRow = sheetNTSage.Rows[1];
                    firstRow.Cells[1].Value = "Emplacement";
                    firstRow.Cells[2].Value = "REF";
                    firstRow.Cells[3].Value = "Quantité";
                    firstRow.Cells[4].Value = "Prix Achat";
                    firstRow.Cells[5].Value = "Total";
                    firstRow.Cells[6].Value = "Désignation";
                    firstRow.Cells[7].Value = "Raison";
                    firstRow.AutoFilter(1);
                    sheetNTSage.Columns.AutoFit();

                    book.Close(true);
                }
                catch (Exception e)
                {
                    log(e.Message);
                }
                finally
                {

                }
            }
        }

        static void chercheNoninventories()
        {
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();
                Workbook book = ExcelApp.Workbooks.Open(fichierCumul);
                Worksheet sheetSaisi = book.Sheets["Saisi"];
                Worksheet sheetRecap = book.Sheets["Recap"];
                // Feuille ontenant les refs qui ont du stock et qui n'ont
                Worksheet sheetNonSaisi = book.Sheets["NonSaisi"];
                sheetNonSaisi.UsedRange.Clear();

                try
                {
                    using (SqlCommand cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = @"SELECT A.AR_Ref, A.AR_Design, DEPO.DP_Code, ARTS.AS_QteSto, AF.AF_RefFourniss, A.AR_PrixAch
                            FROM F_ARTICLE A
                            JOIN F_ARTFOURNISS AF ON A.AR_Ref = AF.AR_Ref AND AF.AF_Principal = 1
                            JOIN F_ARTSTOCK AS ARTS ON A.AR_Ref = ARTS.AR_ref
                            LEFT JOIN F_DEPOTEMPL AS DEPO on ARTS.DP_NoPrincipal = DEPO.DP_No
                            WHERE ARTS.AS_QteSto <> 0";
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            string arRef;
                            int i = 1;
                            while (reader.Read())
                            {
                                arRef = reader["AR_Ref"].ToString().ToUpper();
                                if (arRef == "")
                                {
                                    continue;
                                }
                                Range r = sheetSaisi.Columns[2].Find(arRef);
                                if (r == null)
                                {
                                    // On recherche éventuellement dans le recap si visserie
                                    r = sheetRecap.Columns[2].Find(arRef);
                                }
                                if (r == null)
                                {
                                    decimal qt = (decimal)reader["AS_QteSto"];
                                    string empl = reader["DP_Code"].ToString();
                                    string refFourn = reader["AF_RefFourniss"].ToString();
                                    decimal pxAch = (decimal)reader["AR_PrixAch"];

                                    using (StreamWriter w = File.AppendText(fichierSage))
                                    {
                                        w.WriteLine(strLine, arRef, 0, 0, empl);
                                    }

                                    if (qt < 0)
                                    {
                                        // Si le stock est négatif on force = 0 dans le fichier inventaire.txt
                                        log($"{arRef} STOCK < 0");
                                    } else if (refFourn.Length > 2 && refFourn.Substring(0, 2).Contains("/$")) {
                                        // Si /$
                                        log($"{arRef} REF /$");
                                    } else {
                                        log($"{arRef} NON SAISI");
                                        // Sinon on ajoute dans la feuille NonSaisi
                                        sheetNonSaisi.Cells[i, 1].Value = empl;
                                        sheetNonSaisi.Cells[i, 2].Value = arRef;
                                        sheetNonSaisi.Cells[i, 3].Value = qt;
                                        sheetNonSaisi.Cells[i, 4].Value = reader["AR_Design"].ToString();
                                        sheetNonSaisi.Cells[i, 5].Value = pxAch;
                                        sheetNonSaisi.Cells[i, 6].Value = pxAch * qt;

                                        i++;
                                    }
                                }
                            }
                        }
                    }
                    sheetNonSaisi.Rows[1].Insert();
                    sheetNonSaisi.EnableAutoFilter = true;
                    Range firstRow = sheetNonSaisi.Rows[1];
                    firstRow.Cells[1].Value = "Emplacement";
                    firstRow.Cells[2].Value = "REF";
                    firstRow.Cells[3].Value = "Quantité";
                    firstRow.Cells[4].Value = "Designation";
                    firstRow.Cells[5].Value = "Prix Ach";
                    firstRow.Cells[6].Value = "Total";
                    firstRow.AutoFilter(1);
                    sheetNonSaisi.Columns.AutoFit();

                    book.Close(true);
                }
                catch (Exception e)
                {
                    log(e.Message);
                }

            }
        }

        static void setDefaultEmpl(SqlConnection cnx, string arRef, string empl)
        {
            using (SqlCommand cmdEmpl = cnx.CreateCommand())
            {
                cmdEmpl.CommandText = @"BEGIN TRANSACTION;
                                            UPDATE F_ARTSTOCK SET AS_Principal = 1
                                                , DP_NoPrincipal = (SELECT DP_No FROM F_DEPOTEMPL WHERE DP_Code = @dpCode)
                                            WHERE AR_Ref = @arRef AND AS_Principal = 1
                                            IF @@ROWCOUNT = 0 BEGIN
                                            INSERT INTO F_ARTSTOCK (AR_Ref, DE_No, AS_Principal, DP_NoPrincipal)
                                                VALUES(@arRef, 1, 1, (SELECT DP_No FROM F_DEPOTEMPL WHERE DP_Code = @dpCode));
                                            END COMMIT TRANSACTION;";
                cmdEmpl.Parameters.AddWithValue("@arRef", arRef);
                cmdEmpl.Parameters.AddWithValue("@dpCode", (object)empl ?? DBNull.Value);
                cmdEmpl.ExecuteNonQuery();
            }
        }

        static void log(string message)
        {
            Debug.Print(message);
            Console.WriteLine(message);
            using (StreamWriter w = File.AppendText(fichierLog))
            {
                w.WriteLine("{0} {1} : {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), message);
            }
        }

        static bool IsXLCVErr(object obj)
        {
            return (obj) is Int32;
        }

        static bool IsXLCVErr(object obj, CVErrEnum whichError)
        {
            return (obj is Int32) && ((Int32)obj == (Int32)whichError);
        }
    }
    enum CVErrEnum : Int32
    {
        ErrDiv0 = -2146826281,
        ErrNA = -2146826246,
        ErrName = -2146826259,
        ErrNull = -2146826288,
        ErrNum = -2146826252,
        ErrRef = -2146826265,
        ErrValue = -2146826273
    }
}

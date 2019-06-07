using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using MBCore.Model;
using SyncDb.Model;

namespace SyncDb
{
    static class Program
    {
        //  Repertoire de log
        private static readonly string logDir = "C:\\\\SyncSageDb\\Logs";
        private static readonly string logDate = DateTime.Now.ToString("yyyy-MM-dd");

        static void Main(string[] args)
        {
            EventLog.WriteEntry("Application", "START SYNC SAGE DB");
            Debug.Print("GetCommandLineArgs: {0}", string.Join(", ", args));

            try
            {
                // Check le rep de log
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                // Purge les fichiers de log > 1 semaine
                new DirectoryInfo(logDir).GetFiles("*.log", SearchOption.TopDirectoryOnly)
                    .Where(f => f.LastWriteTime < DateTime.Now.AddDays(-7))
                    .ToList().ForEach(f => {
                        f.Delete();
                });

                var dbList = new DatabaseList();

                // Base source (toujours le premier argument)
                Database sourceDb = dbList.First(d => d.name == args[0]);

                /**
                 * DeployScript sur la première base 
                 * uniquement si un seul argument
                 * cela permet d'isoler le deploiement des scripts sql
                 * sur TARIF et permet de sync TARIF sur les bases de dev
                 */
                if (args.Length == 1)
                {
                    Log("[DISCONNECT USERS]", sourceDb);
                    var decoRepos = new DecoRepository(sourceDb);
                    decoRepos.Log += Log;
                    decoRepos.DeleteUserSessions();

                    Log("[DEPLOY SCRIPTS]", sourceDb);
                    var dsr = new DeployScriptsRepository(sourceDb);
                    dsr.Log += Log;
                    dsr.Process();

                    return;
                }

                // Maj toutes les bases sauf la première
                foreach (var arg in args)
                {
                    var targetDb = dbList.First(d => d.name == arg);
                    if (targetDb.name == args.First())
                    {
                        continue;
                    }
                    Log("[START SYNC]", targetDb);

                    // Déconnecte tout le monde
                    Log("[DISCONNECT USERS]", targetDb);
                    var decoRepos = new DecoRepository(targetDb);
                    decoRepos.Log += Log;
                    if (!decoRepos.DeleteUserSessions())
                    {
                        continue;
                    }

                    Log("[DEPLOY SCRIPTS]", targetDb);
                    var dsr = new DeployScriptsRepository(targetDb);
                    dsr.Log += Log;
                    dsr.Process();

                    Log("[MAJ COMPTA]", targetDb);
                    var cptr = new ComptaRepository(targetDb, sourceDb);
                    cptr.Log += Log;
                    cptr.MajAll();

                    Log("[MAJ FOURN]", targetDb);
                    var fr = new Model.FournisseurRepository(targetDb, sourceDb);
                    fr.Log += Log;
                    fr.MajAll();

                    Log("[MAJ CATALOGUE]", targetDb);
                    var catr = new CatalogueRepository(targetDb, sourceDb);
                    catr.Log += Log;
                    catr.MajAll();

                    Log("[MAJ FAMILLE]", targetDb);
                    var famr = new FamilleRepository(targetDb, sourceDb);
                    famr.Log += Log;
                    famr.MajAll();

                    // TODO résoudre les problèmes d'index entre les bases avant MEP
                    //Log("[MAJ MODELES REGLEMENT]", targetDb);
                    //var mrr = new ModRegRepository(targetDb, sourceDb);
                    //mrr.Log += Log;
                    //mrr.MajAll();
                    Log("[MAJ GLOSSAIRE]", targetDb);
                    var gr = new GlossaireRepository(targetDb, sourceDb);
                    gr.Log += Log;
                    gr.MajAll();

                    var ar = new ArticleRepository(targetDb, sourceDb);
                    ar.Log += Log;
                    Log("[MAJ CONDITIONNEMENTS]", targetDb);
                    ar.MajConditionnements();
                    Log("[MAJ GAMMES]", targetDb);
                    ar.MajGammes();
                    Log("[MAJ ARTICLES]", targetDb);
                    ar.MajAll();

                    Log("[DELETE FAMILLES]", targetDb);
                    famr.DeleteFamilles();

                    Log("DELETE CATALOGUE", targetDb);
                    catr.DeleteCatalogue();

                    Log("[END SYNC]", targetDb);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                EventLog.WriteEntry("Application", e.ToString());
            }
            finally
            {
                EventLog.WriteEntry("Application", "END SYNC SAGE DB");
            }

            Console.WriteLine("FIN");
            Console.Read();
        }

        private static void Log(string message, Database targetDb)
        {
            string logFile = $"{logDir}\\{logDate}-{targetDb.name}.log";
            Debug.Print(message);
            Console.WriteLine(message);
            using (var w = File.AppendText(logFile))
            {
                w.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} {message}");
            }
        }
    }
}

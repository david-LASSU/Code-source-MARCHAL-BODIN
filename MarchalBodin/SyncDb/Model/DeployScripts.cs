using MBCore.Model;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDb.Model
{
    public class DeployScriptsRepository : AbstractRepository
    {
        //  Repertoire de deploiement  des scripts
        public static readonly string depDir = "C:\\\\SyncSageDb\\DeployScripts";

        public DeployScriptsRepository(MBCore.Model.Database TargetDb) : base(TargetDb)
        {
            // Check le rep de deploiement
            if (!Directory.Exists(depDir))
            {
                Directory.CreateDirectory(depDir);
            }
        }

        public override List<Ligne> GetLignes(string PkFilter = null)
        {
            throw new NotImplementedException();
        }

        public override bool MajLigne(Ligne ligne)
        {
            throw new NotImplementedException();
        }

        public void Process()
        {
            try
            {
                using (var cnx = new SqlConnection(targetDb.cnxString))
                {
                    cnx.Open();

#if DEBUG
                    var sqlDir = new DirectoryInfo("\\\\SRVAD01\\COMMUN\\SUPPORT\\SQL\\ScriptsDev");
#else
                    var sqlDir = new DirectoryInfo(string.Format("\\\\{0}\\COMMUN\\SUPPORT\\SQL\\ScriptsProd", targetDb.server.Replace("SRVSQL", "SRVAD")));
#endif
                    foreach (var file in sqlDir.GetFiles())
                    {
                        // Exclusion des fichiers commançant par un underscore
                        if (file.Name[0] == '_')
                        {
                            RaiseLogEvent($"{targetDb} :: Exclusion du fichier {file.Name}");
                            continue;
                        }

                        // Integration systématique des fichier commençant par un tild
                        if (file.Name[0] == '~')
                        {
                            applySqlFile(file, cnx);
                            continue;
                        }

                        // Compare les hash
                        if (file.Extension == ".sql")
                        {
                            // Hash du fichier courant
                            var hash = HashFileGenerator.sha256(file.FullName);
                            // Fichier texte contenant l'ancien hash
                            var depFileName = $"{depDir}\\{targetDb}_" + file.Name.Replace(".sql", ".txt");
                            if (!File.Exists(depFileName))
                            {
                                using (var f = File.CreateText(depFileName))
                                {
                                    f.WriteLine();
                                } 
                            }
                            var oldHash = File.ReadAllLines(depFileName, new UTF8Encoding()).First();
                            if (string.Compare(hash, oldHash) != 0)
                            {
                                try
                                {
                                    // Met à jour le fichier de hash si aucune erreur sql
                                    if(applySqlFile(file, cnx))
                                    {
                                        using (var fw = new StreamWriter(depFileName, false))
                                        {
                                            fw.Write(hash);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    RaiseLogEvent(e.ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                RaiseLogEvent(e.ToString());
            }
        }

        private bool applySqlFile(FileInfo file, SqlConnection cnx)
        {
            try
            {
                RaiseLogEvent($"{targetDb} :: Intégration du fichier {file.Name}");
                var srv = new Server(new ServerConnection(cnx));
                srv.ConnectionContext.ExecuteNonQuery(file.OpenText().ReadToEnd());
                return true;
            }
            catch (Exception e)
            {
                RaiseLogEvent(e.ToString());
                return false;
            }
        }
    }
}

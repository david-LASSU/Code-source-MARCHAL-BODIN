using MBCore.Model;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using WcfServiceLibrary.Model;

namespace WcfServiceLibrary
{
    public class IntermagService : IIntermagService
    {
        private string log = "Application";

        /// <summary>
        /// Vérifie la connexion
        /// </summary>
        /// <returns></returns>
        public bool IsAlive() => true;

        #region Commandes
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="targetDb"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string CreateCommand(string data, string targetDb, int type)
        {
            try
            {
                CommandeRepository commandRepos = new CommandeRepository(targetDb);

                DataSet ds = new DataSet();
                ds.ReadXml(new StringReader(data));
                DataTable dt = ds.Tables[0];

                if (type == 0)
                {
                    return commandRepos.createCommandeDepot(dt, targetDb);
                }
                else if (type == 1)
                {
                    return commandRepos.createCommandeRetro(dt, targetDb);
                }
                else
                {
                    return string.Format("ERREUR Type '{0}'", type);
                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(log, e.ToString(), EventLogEntryType.Error, 100);
                return string.Format("ERREUR : {0}", e.Message);
            }
        }

        /// <summary>
        /// Met à jour la pièce après duplication coté client
        /// </summary>
        /// <param name="clientPiece"></param>
        /// <param name="targetDb"></param>
        /// <param name="fournPiece"></param>
        /// <returns></returns>
        public string SetDoRef(string clientPiece, string targetDb, string fournPiece)
        {
            CommandeRepository commandRepos = new CommandeRepository(targetDb);
            return commandRepos.setDoRef(clientPiece, targetDb, fournPiece);
        }

        public string ToggleVerrou(string db, string doPiece)
        {
            CommandeRepository commandRepos = new CommandeRepository(db);
            return commandRepos.toggleVerrou(doPiece);
        }
        #endregion

        #region Deconnexion
        public User[] GetUsers(string dbName)
        {
            DecoRepository repos = new DecoRepository(dbName);
            return repos.GetUsers();
        }

        public string deleteUserSession(string dbName, string cbSession = null)
        {
            DecoRepository repos = new DecoRepository(dbName);
            return repos.deleteUserSession(cbSession);
        }

        public string killUser(string dbName, string hostName, string ntUserName, string hostProcess, string cbSession)
        {
            if (hostProcess == "")
            {
                return "null";
            }

            int hostProcessId = int.Parse(hostProcess);
            try
            {
                // Tente de killer le programme
                Process.Start("taskkill", $"/S {hostName} /FI \"USERNAME eq {ntUserName}\" /PID {hostProcessId}");

                //if (Process.GetProcessById(hostProcessId, hostName).WaitForExit(5000))
                //{

                //}

                // Supprime depuis cbUserSession
                DecoRepository repos = new DecoRepository(dbName);
                repos.deleteUserSession(cbSession);
                return "Programme en cours d'extinction";
            }
            catch (Exception ex)
            {
                return $"ERREUR: {ex.Message}";
            }
        }
        #endregion
    }
}

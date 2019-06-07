using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Objets100cLib;
using Microsoft.Win32;
using System.Diagnostics;

namespace MBCore.Model
{
    public abstract class DbManager
    {
        /// <summary>
        /// Contient la liste en dur des bases de données
        /// </summary>
        public readonly DatabaseList dbList = new DatabaseList();

        /// <summary>
        /// Retourne le row contenant les parametres de la base
        /// </summary>
        /// <param name="dbName">Nom de la basse</param>
        /// <returns>DataRow Contenant la config de la base</returns>
        public Database getDb(string dbName)
        {
            try
            {
                return dbList.Where(d => d.name == dbName.ToUpper()).First();
            }
            catch (Exception ex)
            {
                throw new Exception($"Database {dbName} non trouvée. Erreur: {ex.Message}");
            }
        }

        /// <summary>
        /// Retourne vrai si la database existe
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public bool dbExiste(string dbName)
        {
            return dbList.Where(d => d.name == dbName.ToUpper()).Any();
        }

        /// <summary>
        /// Retourne le nom du base via le fichier Gescom
        /// </summary>
        /// <param name="fichierGescom">Path vers le fichier Gescomm</param>
        /// <returns></returns>
        public string getDbName(string fichierGescom)
        {
            return fichierGescom.Split('\\').Last().Split('.').First().ToUpper();
        }

        /// <summary>
        /// Retourne la chaine de connexion SQL à partir d'un fichier Gescom
        /// </summary>
        /// <param name="fichierGescomOrDbName">Path du fichier Gescom ou nom de la base</param>
        /// <returns></returns>
        public string getCnxString(string fichierGescomOrDbName)
        {
            string dbName = fichierGescomOrDbName;

            if (dbName.Contains("."))
            {
                dbName = getDbName(fichierGescomOrDbName);
            }

            Database db = getDb(dbName);
            return $"server={db.server};Trusted_Connection=True;Database={db.name};MultipleActiveResultSets=True";
        }

        /// <summary>
        /// Retourne la chaine de connexion SQL à partir d'une instance Gescom ou Compta
        /// </summary>
        /// <param name="bsCial"></param>
        /// <returns></returns>
        public string getCnxString(BSCIALApplication100c bsCial)
        {
            return $"server={bsCial.DatabaseInfo.ServerName};Trusted_Connection=yes;database={bsCial.DatabaseInfo.DatabaseName};MultipleActiveResultSets=True";
        }

        /// <summary>
        /// Retourne la chaine de connexion SQL à partir d'une instance comptable
        /// </summary>
        /// <param name="bsCpta"></param>
        /// <returns></returns>
        public string getCnxString(BSCPTAApplication100c bsCpta)
        {
            return $"server={bsCpta.DatabaseInfo.ServerName};Trusted_Connection=yes;database={bsCpta.DatabaseInfo.DatabaseName};MultipleActiveResultSets=True";
        }

        /// <summary>
        /// Retourne le path du fichier Gescom à partir d'un nom de base donné
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public string getFichierGescom(string dbName)
        {
            return getDb(dbName).gcmFile;
        }

        /// <summary>
        /// 
        /// DEPRECIE utiliser GetSagePath
        /// 
        /// Retourne le path d'exécution de Sage
        /// en fonction de l'environnement de l'ordinateur
        /// </summary>
        /// <param name="programme"></param>
        /// <returns></returns>
        public string getSagePath(string programme = "GESCOM")
        {
            return GetSagePath(programme);
        }

        /// <summary>
        /// Retourne le path d'exécution de Sage
        /// en fonction de l'environnement de l'ordinateur
        /// </summary>
        /// <param name="programme"></param>
        /// <returns></returns>
        public static string GetSagePath(string programme = "GESCOM")
        {
            string exe;
            switch (programme)
            {
                case "GESCOM":
                    exe = "gecomaes.exe";
                    break;
                case "SCD":
                    exe = "scdmaes.exe";
                    break;
                default:
                    throw new NotImplementedException("GetSagePath ne prend pas en compte cette valeur");
            }

            var sagePath =
                (string)Registry.GetValue(
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\" + exe, "", "NONE");

            if (sagePath == "NONE")
            {
                throw new Exception("Path not found");
            }

            return sagePath;
        }
    }
}

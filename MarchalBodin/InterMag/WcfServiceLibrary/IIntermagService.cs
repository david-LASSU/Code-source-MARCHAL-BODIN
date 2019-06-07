using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace WcfServiceLibrary
{
    [ServiceContract]
    public interface IIntermagService
    {
        /// <summary>
        /// Vérifie la connexion
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool IsAlive();

        #region Commandes
        // TODO: Add your service operations here
        /// <summary>
        /// Crée une commande DépôtRetro
        /// </summary>
        /// <param name="data"></param>
        /// <param name="targetDb"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [OperationContract]
        string CreateCommand(string data, string targetDb, int type);

        /// <summary>
        /// Met à jour le champ référence dans la commande
        /// utilisé après duplication coté client d'une cde retro
        /// </summary>
        /// <param name="clientPiece">ABC magasin client</param>
        /// <param name="targetDatabase">db magasin fournisseur</param>
        /// <param name="fournPiece">VBC magasin fournisseur</param>
        /// <returns></returns>
        [OperationContract]
        string SetDoRef(string clientPiece, string targetDatabase, string fournPiece);

        /// <summary>
        /// Vérouille/Dévérouille un document
        /// </summary>
        /// <param name="db"></param>
        /// <param name="doPiece"></param>
        /// <param name="statut"></param>
        /// <returns></returns>
        [OperationContract]
        string ToggleVerrou(string db, string doPiece);
        #endregion

        #region Deconnexion
        /// <summary>
        /// Retourne la liste des session utilisateurs
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        [OperationContract]
        User[] GetUsers(string dbName);

        /// <summary>
        /// Supprime la session utilisateur si cdSession est fourni
        /// Sinon supprime toute les sessions
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="cbSession"></param>
        /// <returns></returns>
        [OperationContract]
        string deleteUserSession(string dbName, string cbSession = null);

        /// <summary>
        /// Déconnecte ou ferme le programme utilisateur à distance
        /// installutil.exe /username=[smo\Administrateur] /password=[xxxxxx] "D:\PC109\Mes documents\Visual Studio 2015\Projects\DeconnexionUtilisateur\DecoServiceHost\bin\Debug\DecoServiceHost.exe"
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="hostName"></param>
        /// <param name="ntUseName"></param>
        /// <param name="hostProcess"></param>
        /// <param name="cbSession"></param>
        /// <returns></returns>
        [OperationContract]
        string killUser(string dbName, string hostName, string ntUseName, string hostProcess, string cbSession);
        #endregion
    }
}

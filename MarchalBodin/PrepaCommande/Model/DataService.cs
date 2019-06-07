using PrepaCommande.Repository;
using System;
using System.Data;
using System.Diagnostics;

namespace PrepaCommande.Model
{
    public class DataService : IDataService
    {
        private CommandeRepository repos = new CommandeRepository();

        public void CloseDocument()
        {
            repos.CloseDocument();
        }

        public void DocumentIsClosed(string doPiece)
        {
            repos.DocumentIsClosed(doPiece);
        }

        /// <summary>
        /// Retourne la liste des articles du fournisseur
        /// fusionnés avec l'apc
        /// </summary>
        /// <param name="DoPiece"></param>
        /// <param name=""></param>
        /// <param name="callback"></param>
        public void GetCommande(string DoPiece, Action<Commande, Exception> callback)
        {
            try
            {
                callback(repos.GetCommande(DoPiece), null);
            }
            catch (Exception e)
            {
                callback(null, e);
            }
        }

        public void GetRefByGencod(string gencod, Action<DataTable, Exception> callback)
        {
            callback(repos.GetRefByGencod(gencod), null);
        }

        public void Import(Commande cmd, Action<bool, Exception> callback)
        {
            try
            {
                callback(repos.Import(cmd), null);
            }
            catch (Exception e)
            {
                callback(false, e);
            }
        }

        public void OpenArticle(string ArRef)
        {
            try
            {
                Process.Start(repos.getSagePath(), $"{repos.fichierGescom}) -u=\"{repos.user}\" -cmd=\"InterroArticle.Show(Masque=Stocks,Article='{ArRef}')\"");
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
        }

        public void ReopenDoc(string doPiece)
        {
            repos.OpenDocument(doPiece);
        }

        public void SaveAll(Commande cmd, Action<bool, Exception> callback)
        {
            callback(repos.SaveAll(cmd), null);
        }
    }
}
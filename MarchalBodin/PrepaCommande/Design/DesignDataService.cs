using System;
using System.Data;
using PrepaCommande.Model;
using PrepaCommande.Repository;

namespace PrepaCommande.Design
{
    public class DesignDataService : IDataService
    {
        private CommandeRepository repos = new CommandeRepository();

        public void CloseDocument()
        {
            throw new NotImplementedException();
        }

        public void DocumentIsClosed(string doPiece)
        {
            throw new NotImplementedException();
        }

        public void GetCommande(string DoPiece, Action<Commande, Exception> callback)
        {
            var cmd = repos.GetCommande(DoPiece);
            cmd.Lignes[0].Qte = 10;
            repos.CloseDocument();
            callback(cmd, null);
        }

        public void GetRefByGencod(string gencod, Action<DataTable, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void Import(Commande cmd, Action<bool, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void OpenArticle(string ArRef)
        {
            throw new NotImplementedException();
        }

        public void ReopenDoc(string doPiece)
        {
            throw new NotImplementedException();
        }

        public void SaveAll(Commande cmd, Action<bool, Exception> callback)
        {
            throw new NotImplementedException();
        }
    }
}
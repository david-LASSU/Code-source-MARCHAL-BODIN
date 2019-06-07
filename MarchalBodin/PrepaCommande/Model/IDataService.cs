using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PrepaCommande.Model
{
    public interface IDataService
    {
        void GetCommande(string DoPiece, Action<Commande, Exception> callback);
        void GetRefByGencod(string gencod, Action<DataTable, Exception> callback);
        void DocumentIsClosed(string doPiece);
        void OpenArticle(string ArRef);
        void ReopenDoc(string doPiece);
        void SaveAll(Commande cmd, Action<bool, Exception> callback);
        void Import(Commande cmd, Action<bool, Exception> callback);
        void CloseDocument();
    }
}

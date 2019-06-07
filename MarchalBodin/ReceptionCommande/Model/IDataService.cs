using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Objets100cLib;
using System.Collections.ObjectModel;
using System.Data;

namespace ReceptionCommande.Model
{
    public interface IDataService
    {
        void GetAll(string DoPiece, Action<Document, Exception> callback);
        void DocumentIsClosed(string doPiece);
        void ReopenDoc(string doPiece);
        void SaveAll(Document document, Action<bool, Exception> callback);
    }
}

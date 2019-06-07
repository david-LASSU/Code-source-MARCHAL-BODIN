using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Divers.Model
{
    public interface IDataService
    {
        void GetDocument(string DoPiece, IEnumerable<Fournisseur> fourns, IEnumerable<Unite> unites, Action<Document, Exception> callback);
        void GetFournisseurs(Action<IEnumerable<Fournisseur>, Exception> callback);
        void GetUnites(Action<IEnumerable<Unite>, Exception> callback);
        void SaveAll(Document document, Action<bool, Exception> callback);
        void CheckDocumentsClosed(Document document);
        void OpenDocument(string DoPiece);
        void Add(Document document, IEnumerable<Unite> unites, Action<bool, Exception> callback);
    }
}

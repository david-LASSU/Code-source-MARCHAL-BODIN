using System;
using MBCore.Model;
using Objets100cLib;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections;
using System.Data;
using System.Diagnostics;

namespace ReceptionCommande.Model
{
    public class DataService : IDataService
    {
        private CommandeRepository repos = new CommandeRepository();

        public void DocumentIsClosed(string doPiece)
        {
            repos.DocumentIsClosed(doPiece);
        }

        public void GetAll(string DoPiece, Action<Document, Exception> callback)
        {
            callback(repos.GetAll(DoPiece), null);
        }

        public void ReopenDoc(string doPiece)
        {
            repos.OpenDocument(doPiece);
        }

        public void SaveAll(Document document, Action<bool, Exception> callback)
        {
            callback(repos.SaveAll(document), null);
        }
    }
}
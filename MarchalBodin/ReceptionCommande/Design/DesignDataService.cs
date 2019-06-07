using System;
using ReceptionCommande.Model;
using MBCore.Model;
using Objets100cLib;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data;

namespace ReceptionCommande.Design
{
    public class DesignDataService : IDataService
    {
        private CommandeRepository repos = new CommandeRepository();

        public void DocumentIsClosed(string doPiece)
        {
            throw new NotImplementedException();
        }

        public void GetAll(string DoPiece, Action<Document, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void ReopenDoc(string doPiece)
        {
            throw new NotImplementedException();
        }

        public void SaveAll(Document document, Action<bool, Exception> callback)
        {
            throw new NotImplementedException();
        }
    }
}
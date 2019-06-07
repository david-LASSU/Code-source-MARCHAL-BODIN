using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Divers.Model
{
    public class DataService : IDataService
    {
        private readonly FournisseurRepository fournRepos = new FournisseurRepository();
        private readonly DiversRepository diversRepos = new DiversRepository();
        private readonly UniteRepository uniteRepos = new UniteRepository();

        public void GetFournisseurs(Action<IEnumerable<Fournisseur>, Exception> callback)
        {
            callback(fournRepos.GetAutoCompleteList(), null);
        }

        public void GetDocument(string DoPiece, IEnumerable<Fournisseur> fourns, IEnumerable<Unite> unites, Action<Document, Exception> callback)
        {
            callback(diversRepos.GetDoc(DoPiece, fourns, unites), null);
        }

        public void GetUnites(Action<IEnumerable<Unite>, Exception> callback)
        {
            callback(uniteRepos.GetAll(), null);
        }

        public void SaveAll(Document document, Action<bool, Exception> callback)
        {
            try
            {
                callback(diversRepos.SaveAll(document), null);
            }
            catch (Exception e)
            {
                callback(false, e);
            }
        }

        public void CheckDocumentsClosed(Document document)
        {
            diversRepos.CheckDocumentsClosed(document);
        }

        public void OpenDocument(string DoPiece)
        {
            diversRepos.OpenDocument(DoPiece);
        }

        public void Add(Document document, IEnumerable<Unite> unites, Action<bool, Exception> callback)
        {
            try
            {
                callback(diversRepos.Add(document, unites), null);
            }
            catch (Exception e)
            {
                callback(false, e);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Divers.Model;
using MBCore.Model;
using System.Linq;

namespace Divers.Design
{
    public class DesignDataService : IDataService
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
            var doc = new Document()
            {
                DoPiece = DoPiece
            };
            Collection<Ligne> lignes = new Collection<Ligne>();
            lignes.Add(new Ligne(diversRepos) {
                Unite = unites.First(u=>u.Intitule == "CENT"),
                Designation = "Test article",
                Quantite = 2,
                Fournisseur =  new Fournisseur { CtNum = "FST00100", Intitule = "Stanley xxxxxxxxxxxxxxxxxxxxxxxxxxxxx"},
                RefFourn = "AZERTYUIOPQS",
                PxBaseAch = 50,
                RemiseAch = 20,
                CoefBase = null,
                CoefNet = 2,
                RemiseVen = 5,
                DesMajDocVen = true,
                Commentaire = "Soleo saepe ante oculos ponere, idque libenter crebris usurpare sermonibus, omnis nostrorum imperatorum, omnis exterarum gentium potentissimorumque populorum, omnis clarissimorum regum res gestas, cum tuis nec contentionum magnitudine nec numero proeliorum nec varietate regionum nec celeritate "
            });
            doc.Lignes = new ObservableCollection<Ligne>(lignes);
            callback(doc, null);
        }

        public void GetUnites(Action<IEnumerable<Unite>, Exception> callback)
        {
            callback(uniteRepos.GetAll(), null);
        }

        public void SaveAll(Document document, Action<bool, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void CheckDocumentsClosed(Document document)
        {
            throw new NotImplementedException();
        }

        public void OpenDocument(string DoPiece)
        {
            throw new NotImplementedException();
        }

        public void Add(Document document, IEnumerable<Unite> unites, Action<bool, Exception> callback)
        {
            throw new NotImplementedException();
        }
    }
}
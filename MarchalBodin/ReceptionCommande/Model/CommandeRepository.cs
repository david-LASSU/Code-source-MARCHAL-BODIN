using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBCore.Model;
using Objets100cLib;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace ReceptionCommande.Model
{
    class CommandeRepository : BaseCialAbstract
    {
        public Document GetAll(string DoPiece)
        {
            if (!openBaseCial())
            {
                throw new Exception("Impossible de se connecter");
            }


            var doc = new Document()
            {
                IBODoc = GetInstance().FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommandeConf, DoPiece)
            };

            var lignes = new ObservableCollection<DocumentLigne>();
            foreach (IBODocumentLigne3 ligne in doc.IBODoc.FactoryDocumentLigne.List)
            {
                if (ligne.Article != null)
                {
                    lignes.Add(new DocumentLigne() {
                        IBOLigne = ligne,
                        RowChecked = false
                    });
                }
            }
            doc.Lignes = new ObservableCollection<DocumentLigne>(lignes.OrderBy(l => l.IBOLigne.Article.AR_Ref).ToList());

            return doc;
        }

        internal bool SaveAll(Document document)
        {
            if (!openBaseCial())
            {
                throw new Exception("Impossible de se connecter à la base");
            }

            foreach (DocumentLigne ligne in document.Lignes)
            {
                ligne.IBOLigne.Write();
                //IBODocumentAchatLigne3 al = (IBODocumentAchatLigne3)ligne.IBOLigne;
                //Debug.Print($"{ligne.IBOLigne.Article.AR_Ref} - {al.DL_QteBL} - {ligne.RowChecked.ToString()}");
            }

            closeBaseCial();
            return true;
        }

        public bool DocumentIsClosed(string doPiece)
        {
            if (openBaseCial())
            {
                try
                {
                    IBODocumentAchat3 doc = GetInstance().FactoryDocumentAchat.ReadPiece(DocumentType.DocumentTypeAchatCommandeConf, doPiece);
                    doc.CouldModified();
                    // Sinon le doc reste ouvert pour OM
                    doc.Write();
                }
                catch (Exception)
                {
                    throw new Exception($"Veuillez fermer le document {doPiece}");
                }
            }

            return true;
        }
    }
}

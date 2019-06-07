using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Objets100cLib;
using System.Collections.ObjectModel;
using System.Linq;
using MBCore.Model;

namespace LiaisonsDocVente.Model
{
    public class Contremarque : INotifyPropertyChanged, IDataErrorInfo
    {
        private ContremarqueRepository _repository = new ContremarqueRepository();
        private readonly Regex _decimalRegex = new Regex("^[0-9\\s]+([\\.|\\,]{1}[0-9]+)?$");
        private readonly Dictionary<string, bool> _validProperties = new Dictionary<string, bool>();
        private IBOFournisseur3 _fourn;
        public bool IsValid
        {
            get
            {
                foreach (KeyValuePair<string, bool> property in _validProperties)
                {
                    if (property.Value == false)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        private bool _rowChecked;
        public bool RowChecked
        {
            get { return _rowChecked; }
            set
            {
                if (value == _rowChecked) { return; }
                _rowChecked = value;

                // Force validation
                OnPropertyChanged("RowChecked");
                OnPropertyChanged("RefFourn");
                OnPropertyChanged("Design");
                OnPropertyChanged("Qte");
                OnPropertyChanged("FournPrinc");
                OnPropertyChanged("PxAch");
                OnPropertyChanged("Remise");
                OnPropertyChanged("Coef");
            }
        }
        /// <summary>
        /// Ref + gamme
        /// Ex 1098-0070/39
        /// </summary>
        public string RefMag { get; set; }
        public string ArRef { get; set; }
        public int AgNo1 { get; set; }
        public int AgNo2 { get; set; }
        public string RefFourn { get; set; }
        public string Design { get; set; }
        public short ArNomencl { get; set; }
        public string Unite { get; set; }
        private double? _qteOrigin;
        public double? QteOrigin
        {
            get { return _qteOrigin; }
            set
            {
                _qteOrigin = value <= 0 ? null : value;
            }
        }
        private double? _qte;
        // TODO Modifier en valeur double comme RelierWindow
        public double? Qte
        {
            get { return _qte; }
            set
            {
                // Quantité à 0 = Ligne de commentaire
                if (QteOrigin == null)
                {
                    _qte = null;
                }
                else
                {
                    _qte = value;
                    // Quantité obligatoire
                    // La qt ne peut pas être supérieure à la qt saisie
                    // ni inférieure à 0
                    if (_qte == null || _qte > QteOrigin || _qte <= 0)
                    {
                        _qte = QteOrigin;
                    }
                }

                OnPropertyChanged("Qte");
            }
        }
        private double? _qteStock;
        public double? QteStock
        {
            get { return _qteStock; }
            set
            {
                _qteStock = value <= 0 ? null : value;
                OnPropertyChanged("QteStock");
            }
        }
        public double QteTotal => (Qte??0) + (QteStock??0);
        private string _fournPrinc;
        public string FournPrinc
        {
            get
            {
                return _fournPrinc;
            }
            set
            {
                if (value == _fournPrinc) return;
                _fournPrinc = value.Trim().ToUpper();

                _fourn = BaseCialAbstract.GetInstance().CptaApplication.FactoryFournisseur.ExistNumero(_fournPrinc) 
                    ? BaseCialAbstract.GetInstance().CptaApplication.FactoryFournisseur.ReadNumero(_fournPrinc) 
                    : null;

                OnPropertyChanged("FournPrinc");
            }
        }
        public List<string> FournList { get; set; }
        private string _selectedFourn;
        public string SelectedFourn
        {
            get { return _selectedFourn; }
            set
            {
                _selectedFourn = value;
                if (_selectedFourn == "Principal")
                {
                    QteStock = 0;
                }
                OnPropertyChanged("SelectedFourn");
                OnPropertyChanged("FournPrinc");
                OnPropertyChanged("IsInterMag");
                OnPropertyChanged("IsQteStockEditable");
            } 
        }
        public string NumPiece { get; set; }
        public int DlNo { get; set; }
        public bool IsUnique => CodeFamille == "UNIQUE";
        public string CodeFamille { get; internal set; }
        public bool IsInterMag => SelectedFourn != "Principal";
        public bool IsArticleDivers => RefMag == "DIVERS";
        public bool IsNotArticleDivers => !IsArticleDivers;
        public bool IsFournListEditable => IsNotArticleDivers && Type != 0 && !IsRelie;
        public bool IsFournPrincEditable => IsCommentaire || IsRemarqueInterne;
        public bool IsQteEditable => !IsCommentaire && !IsRemarqueInterne && IsNotArticleDivers && !IsRelie && !IsUnique;
        public bool IsQteStockEditable => IsInterMag && !IsCommentaire && IsNotArticleDivers && !IsRemarqueInterne && !IsRelie && !IsUnique;
        public bool IsMonoGamme => AgNo1 != 0 && AgNo2 == 0;
        public bool IsDoubleGamme => AgNo1 != 0 && AgNo2 != 0;
        public string Error { get; set; }
        public bool IsCommentaire => string.IsNullOrEmpty(RefMag);
        public bool IsRemarqueInterne => RefMag == "RI";
        public short Type { get; set; }
        public string TexteComplementaire { get; set; }
        private Collection<LiaisonCde> _liaisons;
        public Collection<LiaisonCde> Liaisons
        {
            get { return _liaisons; }
            set
            {
                _liaisons = value;
                OnPropertyChanged("Liaisons");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LiaisonUpdate()
        {
            OnPropertyChanged("IsQteStockEditable");
            OnPropertyChanged("IsFournListEditable");
            OnPropertyChanged("LiaisonsTotal"); 
            OnPropertyChanged("IsQteEditable"); 
            OnPropertyChanged("LiaisonsValid");
            
            Liaisons.ToList().ForEach(l => {
                l.IsEnabled = true;
                if (l.Qte == 0 && Liaisons.Any(ll=>ll.Qte>0))
                {
                    l.IsEnabled = false;
                }
            });
        }
        public double LiaisonsTotal => Liaisons.Sum(l => l.Qte);
        /// <summary>
        /// Il n'est pas possible de relier une ligne de VBC à plusieurs ABC
        /// </summary>
        public bool LiaisonsValid => LiaisonsTotal <= Qte && Liaisons.Count(l => l.Qte > 0) <= 1;
        public bool IsRelie => Liaisons.Sum(l => l.Qte) > 0;
        public bool IsReliable => IsNotArticleDivers && !IsUnique && Type == 1 && Qte != null && QteDispo > 0 && ArNomencl == 0;
        public string IsNomenclatureText => ArNomencl == 0 ? "Non" : "Oui";

        public double QteDispo { get; internal set; }

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;

                // Tant qu'elle n'est pas cochée, la ligne est valide
                if (RowChecked)
                {
                    switch (columnName)
                    {
                        case "Qte":
                            if (Qte == null && !IsCommentaire)
                            {
                                error = "La quantité est obligatoire";
                            }
                            break;
                        case "FournPrinc":
                            if (!IsInterMag)
                            {
                                if (string.IsNullOrEmpty(FournPrinc))
                                {
                                    error = "Fournisseur principal obligatoire";
                                }
                                else if (!BaseCialAbstract.GetInstance().CptaApplication.FactoryFournisseur.ExistNumero(_fournPrinc))
                                {
                                    error = $"Le fournisseur '{FournPrinc}' n'existe pas";
                                }
                            }
                            break;
                    }
                }

                _validProperties[columnName] = error == string.Empty;
                OnPropertyChanged("IsValid");

                return error;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

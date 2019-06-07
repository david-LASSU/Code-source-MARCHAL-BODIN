using System.Collections.Generic;
using System.Linq;
using MBCore.Model;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using System;
using System.IO;

namespace Divers.Model
{
    public class Ligne : ObservableObject, IDataErrorInfo
    {
        #region Init properties
        private readonly DiversRepository repos;
        private readonly Dictionary<string, bool> _validProperties = new Dictionary<string, bool>();
        private string _ArRef;
        public string ArRef {
            get { return _ArRef; }
            set { Set(ref _ArRef, value); }
        }
        public bool IsNew => DlNo == null;
        public bool IsValid => !_validProperties.Values.Contains(false);
        private int? _DlNo;
        public int? DlNo
        {
            get { return _DlNo; }
            set
            {
                Set(ref _DlNo, value);
                RaisePropertyChanged("IsNew");
            }
        }
        //Nombre de contremarques
        public int NbCm { get; internal set; }
        public bool FournisseurEnabled => NbCm == 0 && Domaine == 0;
        public int Domaine { get; internal set; }
        public int Type { get; internal set; }
        private string _Designation;
        public string Designation
        {
            get { return _Designation; }
            set { Set(ref _Designation, value); }
        }

        private double _Quantite;
        public double Quantite
        {
            get { return _Quantite; }
            set
            {
                Set(ref _Quantite, value);
                Calculate();
            }
        }

        private Unite _Unite;
        public Unite Unite
        {
            get { return _Unite; }
            set { Set(ref _Unite, value); }
        }

        private Fournisseur _Fournisseur;
        public Fournisseur Fournisseur
        {
            get { return _Fournisseur; }
            set
            {
                Set(ref _Fournisseur, value);
                RaisePropertyChanged("RefFourn");
            }
        }

        private string _RefFourn;
        public string RefFourn
        {
            get { return _RefFourn; }
            set
            {
                Set(ref _RefFourn, FournisseurRepository.CleanRefFourn(value));
            }
        }

        private double? _PxBaseAch;
        public double? PxBaseAch
        {
            get { return _PxBaseAch; }
            set
            {
                Set(ref _PxBaseAch, value == 0 ? null : value);
                RaisePropertyChanged("PxNetAch");
                RaisePropertyChanged("IsPxNetEnabled");
                Calculate();
            }
        }

        private double? _RemiseAch;
        public double? RemiseAch
        {
            get { return _RemiseAch; }
            set
            {
                if (value >= 100)
                {
                    return;
                }
                Set(ref _RemiseAch, value == 0 ? null : value);
                Calculate();
            }
        }

        private double? _PxNetAch;
        public double? PxNetAch
        {
            get { return _PxNetAch; }
            set
            {
                Set(ref _PxNetAch, value == 0 ? null : value);
                RaisePropertyChanged("PxBaseAch");
                Calculate();
            }
        }

        public bool IsPxNetEnabled => PxBaseAch == null;
        private double? _CoefBase;
        public double? CoefBase
        {
            get { return _CoefBase; }
            set
            {
                Set(ref _CoefBase, value == 0 ? null : value);
                if (_CoefBase != null)
                {
                    CoefNet = null;
                }
                RaisePropertyChanged("CoefNet");
                RaisePropertyChanged("IsCoefNetEnabled");
                Calculate();
            }
        }

        public bool IsCoefBaseEnabled => CoefNet == null;

        private double? _CoefNet;
        public double? CoefNet
        {
            get { return _CoefNet; }
            set
            {
                Set(ref _CoefNet, value == 0 ? null : value);
                if (CoefNet != null)
                {
                    CoefBase = null;
                }
                RaisePropertyChanged("CoefBase");
                RaisePropertyChanged("IsCoefBaseEnabled");
                Calculate();
            }
        }

        public bool IsCoefNetEnabled => CoefBase == null;

        private double? _RemiseVen;
        public double? RemiseVen
        {
            get { return _RemiseVen; }
            set
            {
                if (value >= 100)
                {
                    return;
                }
                Set(ref _RemiseVen, value == 0 ? null : value);
                Calculate();
            }
        }

        private bool? _DesMajDocVen = false;
        public bool? DesMajDocVen
        {
            get { return _DesMajDocVen; }
            set
            {
                Set(ref _DesMajDocVen, value);
                Calculate();
            }
        }

        private string _Commentaire;
        public string Commentaire
        {
            get { return _Commentaire; }
            set { Set(ref _Commentaire, value); }
        }

        private string _SaveResult;
        public string SaveResult
        {
            get { return _SaveResult; }
            set { Set(ref _SaveResult, value); }
        }

        public double? PxUVen => (PxBaseAch != null && CoefBase != null) ? PxBaseAch * CoefBase
            : (PxNetAch != null && CoefNet != null) ? PxNetAch * CoefNet
            : PxNetAch ?? PxBaseAch;
        public double? PxUVenTTC => PxUVen * 1.2;

        public double? PxUVenNet => PxUVen - (PxUVen * (RemiseVen ?? 0) / 100);
        public double? PxUVenNetTTC => PxUVenTTC - (PxUVenTTC * (RemiseVen ?? 0) / 100);

        public double? MontantRemise => PxUVen - PxUVenNet;
        public double? MontantRemiseTTC => PxUVenTTC - PxUVenNetTTC;

        public double? Total => PxUVenNet * Quantite;
        public double? TotalTTC => PxUVenNetTTC * Quantite;
        public string TxtComplementaire => $@"ARTICLE_DIVERS:
Designation:{Designation}
Quantite:{Quantite}
Num Fourn:{Fournisseur.CtNum}
Ref Fourn:{RefFourn}
Prix de base:{PxBaseAch}
Remise Fourn:{RemiseAch}
Prix Net:{PxNetAch}
Coef Base:{CoefBase}
Coef Net:{CoefNet}
Remise Ven:{RemiseVen}
Desactive maj doc ven:{(DesMajDocVen == true ? "oui" : "non")}
*************** DEBUT COMMENTAIRE ***************
{Commentaire}
*************** FIN COMMENTAIRE ***************";

        public string Error { get { return this[string.Empty]; } }

        public bool IsRelicat { get; internal set; }

        public string this[string columnName]
        {
            get
            {
                string error = string.Empty;
                switch (columnName)
                {
                    case "Fournisseur":
                        if (Fournisseur == null)
                        {
                            error = "Fournisseur obligatoire (laissez vide pour ignorer la ligne)";
                        }
                        break;
                    case "RefFourn":
                        if ((RefFourn??string.Empty).Length > 19)
                        {
                            error = "La référence fournisseur doit faire 19 caractères max";
                        }
                        string arRef = repos.ExistRefFourn(this);
                        if (arRef.Length > 0)
                        {
                            error = $"L'article {arRef} existe déjà pour cette référence.";
                        }
                        break;
                    case "Quantite":
                        if (Quantite <= 0)
                        {
                            error = "La quantité est obligatoire";
                        }
                        break;
                }

                _validProperties[columnName] = error?.Length == 0;
                RaisePropertyChanged("IsValid");

                return error;
            }
        }
        #endregion

        public Ligne(DiversRepository dr)
        {
            repos = dr;
        }

        public void ValidAllProperties()
        {
            foreach (System.Reflection.PropertyInfo item in typeof(Ligne).GetProperties())
            {
                string e = this[item.Name];
            }
        }

        public void Calculate()
        {
            double? pxnetach = PxBaseAch - (PxBaseAch * (RemiseAch ?? 0) / 100);
            if (PxBaseAch != null && PxNetAch != pxnetach)
            {
                PxNetAch = pxnetach;
            }

            RaisePropertyChanged("PxUVen");
            RaisePropertyChanged("PxUVenNet");
            RaisePropertyChanged("MontantRemise");
            RaisePropertyChanged("Total");
            RaisePropertyChanged("PxUVenTTC");
            RaisePropertyChanged("PxUVenNetTTC");
            RaisePropertyChanged("MontantRemiseTTC");
            RaisePropertyChanged("TotalTTC");
        }

        public void SaveResultTo(bool res)
        {
            SaveResult = (res == true ? "Enregistré" : "Ignoré") + " à " + DateTime.Now.ToString("HH:mm:ss");
        }

        public void ParseTxtComplementaire(string txt)
        {
            if (!txt.Contains("ARTICLE_DIVERS:"))
            {
                return;
            }
            using (StringReader r = new StringReader(txt))
            {
                string[] line;
                string l, key, val;
                while ((l = r.ReadLine()) != null)
                {
                    line = l.Split(':');
                    key = line[0];
                    val = line.Length > 1 ? line[1].Trim() : null;

                    switch (key)
                    {
                        case "Num Fourn":
                            Fournisseur = val?.Length == 0 ? null : new Fournisseur() { CtNum = val };
                            break;
                        case "Ref Fourn":
                            RefFourn = val?.Length == 0 ? null : val;
                            break;
                        case "Prix de base":
                            PxBaseAch = val?.Length == 0 ? (double?)null : double.Parse(val.Replace('.', ','));
                            break;
                        case "Remise Fourn":
                            RemiseAch = val?.Length == 0 ? (double?)null : double.Parse(val.Replace('.', ','));
                            break;
                        case "Prix Net":
                            PxNetAch = val?.Length == 0 ? (double?)null : double.Parse(val.Replace('.', ','));
                            break;
                        case "Coef Base":
                            CoefBase = val?.Length == 0 ? (double?)null : double.Parse(val.Replace('.', ','));
                            break;
                        case "Coef Net":
                            CoefNet = val?.Length == 0 ? (double?)null : double.Parse(val.Replace('.', ','));
                            break;
                        case "Remise Ven":
                            RemiseVen = val?.Length == 0 ? (double?)null : double.Parse(val.Replace('.', ','));
                            break;
                        case "Desactive maj doc ven":
                            DesMajDocVen = val == "oui";
                            break;
                    }
                }
            }
            string startTag = "*************** DEBUT COMMENTAIRE ***************";
            int startIndex = txt.IndexOf(startTag) + +startTag.Length;
            int endIndex = txt.IndexOf("*************** FIN COMMENTAIRE ***************", startIndex);
            Commentaire = txt.Substring(startIndex, endIndex - startIndex).Trim('\r', '\n');
        }
    }
}

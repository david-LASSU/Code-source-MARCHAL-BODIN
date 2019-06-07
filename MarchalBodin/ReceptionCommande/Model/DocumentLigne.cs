using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Objets100cLib;

namespace ReceptionCommande.Model
{
    public class DocumentLigne : ObservableObject
    {
        private bool _rowChecked;
        public bool RowChecked
        {
            get { return _rowChecked; }
            set { Set(ref _rowChecked, value); }
        }
        public IBODocumentLigne3 IBOLigne { get; set; }
    }
}

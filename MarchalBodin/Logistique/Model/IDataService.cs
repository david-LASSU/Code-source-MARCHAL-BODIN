using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Objets100cLib;
using System.Collections.ObjectModel;

namespace Logistique.Model
{
    public interface IDataService
    {
        void GetEmplacement(string dpCode, Action<IBODepotEmplacement, Exception> callback);

        void GetArticles(IBODepotEmplacement emplacement, Action<Collection<Article>, Exception> callback);

        void SetDefaultEmpl(string str, IBODepotEmplacement emplacement, Action<IBOArticle3, Exception> callback);

        void DeclareStock(double noteStock, Article article, Action<Exception> callback);
    }
}

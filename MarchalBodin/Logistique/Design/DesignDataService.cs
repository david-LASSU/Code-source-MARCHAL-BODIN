using System;
using System.Collections.ObjectModel;
using Logistique.Model;
using Objets100cLib;

namespace Logistique.Design
{
    public class DesignDataService : IDataService
    {
        public void DeclareStock(double noteStock, Article article, Action<Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void GetArticles(IBODepotEmplacement emplacement, Action<Collection<Article>, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void GetEmplacement(string dpCode, Action<IBODepotEmplacement, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void SetDefaultEmpl(string str, IBODepotEmplacement emplacement, Action<IBOArticle3, Exception> callback)
        {
            throw new NotImplementedException();
        }
    }
}
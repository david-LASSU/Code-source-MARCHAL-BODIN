using System;
using Objets100cLib;
using Logistique.Repository;
using System.Collections.ObjectModel;

namespace Logistique.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem("Welcome to MVVM Light");
            callback(item, null);
        }

        LogistiqueRepository repos = new LogistiqueRepository();

        public void GetEmplacement(string dpCode, Action<IBODepotEmplacement, Exception> callback)
        {
            try
            {
                callback(repos.GetEmplacement(dpCode), null);
            }
            catch (Exception e)
            {
                callback(null, e);
            }
        }

        public void GetArticles(IBODepotEmplacement emplacement, Action<Collection<Article>, Exception> callback)
        {
            try
            {
                callback(repos.GetArticles(emplacement), null);
            }
            catch (Exception e)
            {
                callback(null, e);
            }
        }

        public void SetDefaultEmpl(string str, IBODepotEmplacement emplacement, Action<IBOArticle3, Exception> callback)
        {
            try
            {
                callback(repos.SetDefaultEmpl(str, emplacement), null);
            }
            catch (Exception e)
            {
                callback(null, e);
            }
        }

        public void DeclareStock(double noteStock, Article article, Action<Exception> callback)
        {
            try
            {
                repos.DeclareStock(noteStock, article);
                callback(null);
            }
            catch (Exception e)
            {
                callback(e);
            }
        }
    }
}
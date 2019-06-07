using System;
using System.Collections.Generic;
using MBCore.Model;
using SyncDb.Model;
using System.Collections;
using System.Linq;

namespace SyncDbUi.Model
{
    public class DataService : IDataService
    {
        private Database targetDb;
        private Database sourceDb;
        private readonly Dictionary<string, AbstractRepository> repos = new Dictionary<string, AbstractRepository>();

        public event AbstractRepository.LogEventHandler Log;

        public void Load(string ObjectName, string PkFilter, Action<IEnumerable<Ligne>, Exception> callback)
        {
            callback(repos[ObjectName].GetLignes(PkFilter), null);
        }

        public void Save(string ObjectName, IList selectedItems, ICollection<Ligne> items, Action callback)
        {
            // Si article on force la maj Gammes + Cond avant
            if (ObjectName == "ARTICLE")
            {
                var artRepos = ((ArticleRepository)repos["ARTICLE"]);
                artRepos.MajConditionnements();
                artRepos.MajGammes();
            }

            selectedItems.Cast<Ligne>().ToList().ForEach((l) => {
                if (repos[ObjectName].MajLigne(l))
                {
                    App.Current.Dispatcher.Invoke(delegate { items.Remove(l); });
                };
            });
            callback();
        }

        public void SetDatabases(Database TargetDb, Database SourceDb)
        {
            targetDb = TargetDb;
            sourceDb = SourceDb;

            repos.Add("ARTICLE", new ArticleRepository(TargetDb, SourceDb));
            repos["ARTICLE"].Log += RaiseLogEvent;
            repos.Add("FAMILLE", new FamilleRepository(TargetDb, SourceDb));
            repos["FAMILLE"].Log += RaiseLogEvent;
            repos.Add("FOURNISSEUR", new SyncDb.Model.FournisseurRepository(TargetDb, SourceDb));
            repos["FOURNISSEUR"].Log += RaiseLogEvent;
            repos.Add("CATALOGUE", new CatalogueRepository(TargetDb, SourceDb));
            repos["CATALOGUE"].Log += RaiseLogEvent;
        }

        public IList<string> GetSyncablesObjects() => repos.Keys.ToArray();

        public delegate void EventHandler(string message, Database targetDb);
        protected void RaiseLogEvent(string message, Database targetDb)
        {
            Log?.Invoke(message, targetDb);
        }
    }
}
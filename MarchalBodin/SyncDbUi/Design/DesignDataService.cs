using System;
using System.Collections;
using System.Collections.Generic;
using MBCore.Model;
using SyncDb.Model;
using SyncDbUi.Model;

namespace SyncDbUi.Design
{
    public class DesignDataService : IDataService
    {
#pragma warning disable 67
        public event AbstractRepository.LogEventHandler Log;
#pragma warning restore 67

        public IList<string> GetSyncablesObjects()
        {
            throw new NotImplementedException();
        }

        public void Load(string ObjectName, string PkFilter, Action<IEnumerable<Ligne>, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void Save(string ObjectName, IList selectedItems, ICollection<Ligne> items, Action callback)
        {
            throw new NotImplementedException();
        }

        public void SetDatabases(Database TargetDb, Database SourceDb)
        {
            throw new NotImplementedException();
        }
    }
}
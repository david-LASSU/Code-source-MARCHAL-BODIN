using MBCore.Model;
using SyncDb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SyncDb.Model.AbstractRepository;
using System.Collections;

namespace SyncDbUi.Model
{
    public interface IDataService
    {
        void Load(string ObjectName, string PkFilter, Action<IEnumerable<Ligne>, Exception> callback);
        void SetDatabases(Database TargetDb, Database SourceDb);
        IList<string> GetSyncablesObjects();
        event LogEventHandler Log;

        void Save(string ObjectName, IList selectedItems, ICollection<Ligne> items, Action callback);
    }
}

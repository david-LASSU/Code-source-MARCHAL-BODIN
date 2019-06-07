using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MBCore.Model;
using System.Data.SqlClient;

namespace SyncDb.Model
{
    class DecoRepository : AbstractRepository
    {
        public DecoRepository(Database TargetDb) : base(TargetDb)
        {
            targetDb = TargetDb;
        }

        public bool DeleteUserSessions()
        {
            try
            {
                RaiseLogEvent($"{targetDb} :: Deconnection utilisateurs");
                using (var cnx = new SqlConnection(targetDb.cnxString))
                {
                    cnx.Open();

                    using (var cmd = cnx.CreateCommand())
                    {
                        // Deconnecte tout le monde (session fantôme)
                        cmd.CommandText = $@"
                            DBCC cbsqlxp(free);
                            DELETE FROM cbMessage;
                            DELETE FROM cbNotification;
                            DELETE FROM cbRegfile;
                            DELETE FROM cbRegMessage;
                            DELETE FROM cbRegUser;
                            DELETE FROM cbUserSession;";

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                RaiseLogEvent(e.ToString());
                return false;
            }
        }

        public override List<Ligne> GetLignes(string PkFilter = null)
        {
            throw new NotImplementedException();
        }

        public override bool MajLigne(Ligne ligne)
        {
            throw new NotImplementedException();
        }
    }
}

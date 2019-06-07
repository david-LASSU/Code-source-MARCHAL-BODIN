using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace WcfServiceLibrary.Model
{
    class DecoRepository : BaseCialAbstract
    {
        public DecoRepository(string targetDb)
        {
            setDefaultParams(getDb(targetDb).gcmFile, ADMINUSR);
        }

        public User[] GetUsers()
        {
            Collection<User> users = new Collection<User>();
            using (SqlConnection cnx = new SqlConnection(cnxString))
            {
                cnx.Open();

                using (SqlCommand cmd = cnx.CreateCommand())
                {
                    cmd.CommandText = $@"SELECT CB.cbSession, S.session_id, S.host_process_id, S.program_name, CB.CB_Type, CB.cbUserName, S.host_name, S.nt_user_name
                                        FROM cbUserSession CB
                                        LEFT JOIN sys.dm_exec_sessions S ON S.session_id = CB.cbSession AND DB_ID(DB_NAME()) = S.database_id AND S.program_name LIKE '%sage%'";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return users.ToArray();
                        }

                        while (reader.Read())
                        {
                            users.Add(new User()
                            {
                                cbSession = reader["cbSession"].ToString().Trim(),
                                sessionId = reader["session_id"].ToString().Trim(),
                                hostProcessId = reader["host_process_id"].ToString().Trim(),
                                programName = reader["program_name"].ToString().Trim(),
                                CbType = reader["CB_Type"].ToString().Trim(),
                                cbUserName = reader["cbUserName"].ToString().Trim(),
                                hostname = reader["host_name"].ToString().Trim(),
                                ntUsername = reader["nt_user_name"].ToString().Trim()
                            });
                        }
                    }
                }
            }
            return users.ToArray();
        }

        public string deleteUserSession(string cbSession = null)
        {
            try
            {
                using (SqlConnection cnx = new SqlConnection(cnxString))
                {
                    cnx.Open();
                    using (SqlCommand cmd = cnx.CreateCommand())
                    {
                        if (cbSession == null)
                        {
                            // Deconnecte tout le monde (session fantôme)
                            cmd.CommandText = $@"USE {dbName};
                            DBCC cbsqlxp(free);
                            DELETE FROM cbMessage;
                            DELETE FROM cbNotification;
                            DELETE FROM cbRegfile;
                            DELETE FROM cbRegMessage;
                            DELETE FROM cbRegUser;
                            DELETE FROM cbUserSession;";

                            cmd.ExecuteNonQuery();

                            return "Les sessions ont bien été supprimées";
                        }
                        else
                        {
                            // Deconnecte un seul utilisateur
                            cmd.CommandText = $@"USE {dbName};
                            DBCC cbsqlxp(free);
                            DELETE FROM cbMessage WHERE cbSession IN (SELECT session_id FROM sys.dm_exec_sessions WHERE host_process_id IN (SELECT host_process_id FROM sys.dm_exec_sessions WHERE session_id=@cbSession));
                            DELETE FROM cbNotification WHERE cbSession IN (SELECT session_id FROM sys.dm_exec_sessions WHERE host_process_id IN (SELECT host_process_id FROM sys.dm_exec_sessions WHERE session_id=@cbSession));
                            DELETE FROM cbRegfile WHERE cbSession IN (SELECT session_id FROM sys.dm_exec_sessions WHERE host_process_id IN (SELECT host_process_id FROM sys.dm_exec_sessions WHERE session_id=@cbSession));
                            DELETE FROM cbRegMessage WHERE cbSession IN (SELECT session_id FROM sys.dm_exec_sessions WHERE host_process_id IN (SELECT host_process_id FROM sys.dm_exec_sessions WHERE session_id=@cbSession));
                            DELETE FROM cbRegUser WHERE cbSession IN (SELECT session_id FROM sys.dm_exec_sessions WHERE host_process_id IN (SELECT host_process_id FROM sys.dm_exec_sessions WHERE session_id=@cbSession));
                            DELETE FROM cbUserSession WHERE cbSession IN (SELECT session_id FROM sys.dm_exec_sessions WHERE host_process_id IN (SELECT host_process_id FROM sys.dm_exec_sessions WHERE session_id=@cbSession));";

                            cmd.Parameters.AddWithValue("@cbSession", cbSession);
                            cmd.ExecuteNonQuery();

                            //cmd.Parameters.Clear();
                            //cmd.CommandText = $"KILL {sessionId};";
                            //cmd.ExecuteNonQuery();

                            return "La session a bien été supprimée";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return $"ERREUR : {ex.Message}";
            }
        }
    }
}

using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SyncDb.Model
{
    public abstract class AbstractRepository
    {
        public event LogEventHandler Log;
        public delegate void LogEventHandler(string message, Database targetDb);
        public readonly string Hostname = System.Net.Dns.GetHostName();
        public const int CmdTimeOut = 240;
        protected Database targetDb;
        protected Database sourceDb;

        public AbstractRepository(Database TargetDb, Database SourceDb = null)
        {
            targetDb = TargetDb;
            sourceDb = SourceDb;
        }

        public virtual void MajAll()
        {
            try
            {
                var lignes = GetLignes();
                RaiseLogEvent($"{lignes.Count} objets à mettre à jour");
                foreach (Ligne ligne in lignes)
                {
                    MajLigne(ligne);
                }
            }
            catch (Exception e)
            {
                RaiseLogEvent(e.ToString());
            }
        }

        /// <summary>
        /// Must be overrided
        /// </summary>
        /// <param name="ligne"></param>
        public abstract bool MajLigne(Ligne ligne);

        /// <summary>
        /// Must be overrided
        /// </summary>
        /// <returns></returns>
        public abstract List<Ligne> GetLignes(string PkFilter = null);

        public void FilterGetLignes(SqlCommand cmd, string PkFilter = null)
        {
            string filter = string.Empty;
            if (PkFilter != null)
            {
                filter = $"AND L.PkValue LIKE '%'+@ref+'%'";
                cmd.Parameters.AddWithValue("@ref", PkFilter);
            }
            cmd.CommandText = cmd.CommandText.Replace("%PKFILTER%", filter);
        }

        public void UpdateSyncState(Ligne ligne)
        {
            try
            {
                using (var cnx = new SqlConnection(targetDb.cnxString))
                {
                    cnx.Open();
                    using (var cmd = cnx.CreateCommand())
                    {
                        cmd.CommandText = $@"UPDATE {targetDb.dboChain}.[MB_SYNCSTATE] SET [LastUpdate] = GETDATE() WHERE [ObjectName] = @ObjectName AND [PkValue] = @PkValue
                                            IF @@ROWCOUNT = 0
                                            INSERT INTO {targetDb.dboChain}.[MB_SYNCSTATE] (ObjectName, PkValue) VALUES (@ObjectName, @PkValue)";
                        cmd.Parameters.AddWithValue("@ObjectName", ligne.ObjectName);
                        cmd.Parameters.AddWithValue("@PkValue", ligne.PKValue);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                RaiseLogEvent(e.ToString());
            }
        }

        protected void RaiseLogEvent(string message)
        {
            Log?.Invoke(message, targetDb);
        }

        protected string GetViewSelectAliases(List<string> vCols)
        {
            List<string> str = new List<string>();
            foreach (string c in vCols)
            {
                str.Add($"L.{c} AS L_{c}, R.{c} AS R_{c} ");
            }

            return string.Join(",", str.ToArray());
        }

        protected void InsertRowFromReader(string table, SqlDataReader reader, SqlTransaction transaction)
        {
            SqlConnection cnxTarget = transaction.Connection;
            string cleanSqlParam;
            string logString = $"INSERT INTO {cnxTarget.Database}.[dbo].[{table}]";

            using (var cmd = cnxTarget.CreateCommand())
            {
                cmd.Transaction = transaction;

                string fieldStr = string.Empty;
                string valueStr = string.Empty;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (i > 0)
                    {
                        fieldStr += ", ";
                        valueStr += ", ";
                    }

                    cleanSqlParam = getCleanSqlParam(reader.GetName(i));
                    fieldStr += $"[{reader.GetName(i)}]";
                    valueStr += $"@{cleanSqlParam}";
                    cmd.Parameters.AddWithValue(cleanSqlParam, reader[i]);
                    logString += $" [{reader.GetName(i)} = {reader[i]}]";
                }
                cmd.CommandText = $"INSERT INTO [{cnxTarget.Database}].[dbo].[{table}] ({fieldStr}) VALUES ({valueStr})";
                cmd.CommandTimeout = CmdTimeOut;
                RaiseLogEvent(logString);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Met à jour une ligne de table en prenant le premier champs en clé primaire
        /// </summary>
        /// <param name="table"></param>
        /// <param name="reader"></param>
        /// <param name="transaction"></param>
        /// <param name="updBlackList"></param>
        protected void UpdateRowFromReader(string table, SqlDataReader reader, SqlTransaction transaction, List<string>updBlackList = null)
        {
            var cnxTarget = transaction.Connection;
            string cleanSqlParam, logString;
            using (var cmd = cnxTarget.CreateCommand())
            {
                cmd.Transaction = transaction;
                logString = $"UPDATE {cnxTarget.Database}.[dbo].[{table}] SET ";
                cmd.CommandText = $"UPDATE {cnxTarget.Database}.[dbo].[{table}] SET ";
                for (int i = 1; i < reader.FieldCount; i++)
                {
                    if (null != updBlackList && updBlackList.Contains(reader.GetName(i)))
                    {
                        continue;
                    }

                    cleanSqlParam = getCleanSqlParam(reader.GetName(i));
                    cmd.CommandText += $"[{reader.GetName(i)}] = @{cleanSqlParam}";
                    cmd.Parameters.AddWithValue(cleanSqlParam, reader[i]);
                    logString += $"[{reader.GetName(i)}] = " + reader[i].ToString();
                    if (i < reader.FieldCount - 1)
                    {
                        cmd.CommandText += ", ";
                        logString += ", ";
                    }
                }
                cmd.CommandText += $" WHERE [{reader.GetName(0)}] = @PKey";
                cmd.Parameters.AddWithValue("@PKey", reader[0]);
                cmd.CommandTimeout = CmdTimeOut;
                logString += $" WHERE [{reader.GetName(0)}] = {reader[0]}";
                RaiseLogEvent(logString);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Construit une commande de table select d'une table source
        /// en utilisant une liste de champs dont le premier élément est la clé primaire
        /// </summary>
        /// <param name="cnx"></param>
        /// <param name="table"></param>
        /// <param name="fields"></param>
        /// <param name="pkValue"></param>
        /// <param name="andPart"></param>
        /// <returns></returns>
        protected SqlCommand GetSelectCmdFromCnx(SqlConnection cnx, string table, List<string> fields, string pkValue, string andPart = "")
        {
            var cmd = cnx.CreateCommand();
            string fieldsStr = string.Join("],[", fields.ToArray());
            andPart = andPart == "" ? "" : $" AND {andPart}";
            cmd.CommandText = $"SELECT [{fieldsStr}] FROM [{cnx.DataSource}].[{cnx.Database}].[dbo].[{table}] WHERE [{fields.First()}] = @value{andPart}";
            cmd.Parameters.AddWithValue("@value", pkValue);
            cmd.CommandTimeout = CmdTimeOut;

            return cmd;
        }

        /// <summary>
        /// Automatise le process de copier les données d'une table source vers une table destination
        /// </summary>
        /// <param name="cnxSource"></param>
        /// <param name="table"></param>
        /// <param name="fields"></param>
        /// <param name="pkValue"></param>
        /// <param name="transaction"></param>
        /// <param name="andPart"></param>
        protected void CopySourceToTarget(SqlConnection cnxSource, string table, List<string> fields, string pkValue, SqlTransaction transaction, string andPart = "")
        {
            using (var cmdSource = GetSelectCmdFromCnx(cnxSource, table, fields, pkValue, andPart))
            {
                using (var reader = cmdSource.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            InsertRowFromReader(table, reader, transaction);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Delete from table where pKey = value
        /// </summary>
        /// <param name="table"></param>
        /// <param name="transaction"></param>
        /// <param name="pKey"></param>
        /// <param name="value"></param>
        /// <param name="andPart"></param>
        protected void Delete(string table, SqlTransaction transaction, string pKey, string value, string andPart = "")
        {
            var cnx = transaction.Connection;
            andPart = andPart == "" ? "" : $" AND {andPart}";
            RaiseLogEvent($"DELETE FROM {cnx.Database}.[dbo].[{table}] WHERE {pKey} = {value}{andPart}");

            using (var cmd = cnx.CreateCommand())
            {
                cmd.Transaction = transaction;
                cmd.CommandText = $"DELETE FROM {cnx.Database}.[dbo].[{table}] WHERE {pKey} = @value{andPart}";
                cmd.Parameters.AddWithValue("@value", value);
                cmd.CommandTimeout = CmdTimeOut;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// TODO use regex
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string getCleanSqlParam(string fieldName)
        {
            return fieldName.Replace(" ", "").Replace("°", "");
        }
    }
}

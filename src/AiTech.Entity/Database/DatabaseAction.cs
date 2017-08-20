using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;


namespace AiTech.LiteOrm.Database
{
    public class DatabaseAction
    {

        public static bool ExecuteDeleteQuery<T>(string encoder, IEnumerable<T> items, SqlConnection db, SqlTransaction trn = null) where T : Entity
        {
            CheckDbConnection(db);
            var tableName = items.First().TableName;
            var primaryKey = items.First().PrimaryKey;

            var sql = $" Delete from [{tableName}] where {primaryKey} in @Ids";
            return db.Execute(sql, new { Ids = items.Select(_ => _.Id).ToArray() }, trn) != 0;
        }


        internal static bool ExecuteCommand<T>(SqlCommand cmd, T item, string errorDescription) where T : Entity
        {
            CheckDbConnection(cmd.Connection);

            try
            {
                if (cmd.CommandText.Length == 0) return false;
                using (cmd)
                {
                    CleanParameters(cmd);

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        UpdateItemRecordInfo(item, reader);
                        return reader.HasRows;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Error Saving " + errorDescription, ex);
            }
        }


        private static void CleanParameters(SqlCommand cmd)
        {
            var query = cmd.CommandText;

            if (string.IsNullOrEmpty(query)) return;

            for (var i = cmd.Parameters.Count - 1; i >= 0; i--)
            {
                var param = cmd.Parameters[i];

                if (!query.Contains(param.ParameterName))
                    cmd.Parameters.Remove(param);
            }
        }


        public static void CheckDbConnection(SqlConnection db)
        {
            if (db == null) throw new Exception("DbConnection NOT Set. Use SetDbConnection Property");
        }

        internal static void UpdateItemRecordInfo(Entity item, SqlDataReader reader)
        {
            if (!reader.Read()) return; //throw new Exception("Error Inserting New Item");

            var col = -1;

            if (reader.HasField("Id", out col))
                item.Id = reader.GetInt32(col);

            if (reader.HasField("Created", out col))
                item.Created = reader.GetDateTime(col);

            if (reader.HasField("CreatedBy", out col))
                item.CreatedBy = reader[col].ToString();

            if (reader.HasField("Modified", out col))
                item.Modified = reader.GetDateTime(col);

            if (reader.HasField("ModifiedBy", out col))
                item.ModifiedBy = reader[col].ToString();
        }


    }


    public static class DatabaseExtension
    {
        public static bool HasField(this SqlDataReader reader, string name, out int col)
        {
            try
            {
                col = reader.GetOrdinal(name);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                col = -1;
                return false;
            }
        }
    }
}

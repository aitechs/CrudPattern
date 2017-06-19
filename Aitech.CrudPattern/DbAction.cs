using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using Dapper.Contrib.Extensions;

namespace AiTech.Utility
{
    public class DbAction
    {
        public static bool ExecuteDeleteQuery<T>(string encoder, T item, SqlConnection db, SqlTransaction trn = null) where T: CrudPattern.Entity
        {
            CheckDbConnection(db);

            var sql = $" Delete from [{item.TableName}] where {item.PrimaryKey} = @Id";
            return db.Execute(sql, item, trn) != 0;
        }

        public static bool ExecuteInsertQuery<T>(string encoder, T item, SqlConnection db, SqlTransaction trn = null) where T:CrudPattern.Entity
        {
            CheckDbConnection(db);
            try
            {
                item.CreatedBy = encoder;
                item.ModifiedBy = encoder;

                item.Id = Convert.ToInt32(db.Insert(item, trn));
                if (item.Id == 0) throw new Exception("Error Inserting Data");

                UpdateItemModifiedDates<T>(ref item, db, trn);
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("duplicate"))
                    throw new Exception("Duplicate Record Error!");
                throw;
            }
        }

        public static bool ExecuteUpdateQuery<T>(string encoder, T item, SqlConnection db, SqlTransaction trn = null) where T : CrudPattern.Entity
        {
            CheckDbConnection(db);
            var result = 0;

            try
            {
                var sql = item.GetUpdateSQLQuery();
                if (!String.IsNullOrEmpty(sql))
                {
                    item.ModifiedBy = encoder;
                    result = db.Execute(sql, item, trn);

                    if (result == 0) throw new Exception("Record NOT saved");
                    UpdateItemModifiedDates<T>(ref item, db, trn);
                }
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("duplicate"))
                    throw new Exception("Duplicate Record Error!");
                throw;
            }
        }

        public static void CheckDbConnection(SqlConnection db)
        {
            if (db == null) throw new Exception("DbConnection NOT Set. Use SetDbConnection Property");
        }

        internal static void UpdateItemModifiedDates<T>(ref T item, SqlConnection db, SqlTransaction trn = null) where T: CrudPattern.Entity
        {
            var sql = string.Format("Select Created, Modified, CreatedBy, ModifiedBy from {0} where {1} = {2}", item.TableName, item.PrimaryKey, item.Id);
            db.Query(sql, item, transaction: trn).FirstOrDefault();
        }
    }
}

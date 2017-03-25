using AiTech.CrudPattern;
using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aitech.CrudPattern
{


    public class QueryManager<TEntity> : ITransaction where TEntity : Entity
    {
        protected SqlConnection DbConnection = null;
        protected SqlTransaction DbTransaction = null;

        public delegate void AfterDbProcessHandler(TEntity item, SqlConnection db, SqlTransaction trn);
        public delegate void ItemErrorHandler(TEntity item, out string errorDescription);

        public event AfterDbProcessHandler AfterDbDeleteItem;
        public event AfterDbProcessHandler AfterDbUpdateItem;
        public event AfterDbProcessHandler AfterDbInsertItem;
        public event AfterDbProcessHandler AfterRowChanged;
        public event ItemErrorHandler ItemError;

        protected EntityCollection<TEntity> RecordItemCollection;
        protected string Encoder;

        public QueryManager(string encoder)
        {
            Encoder = encoder;
        }

        public void SetConnection(SqlConnection connection = null, SqlTransaction transaction = null)
        {
            DbConnection = connection;
            DbTransaction = transaction;
        }


        public void SetItems(EntityCollection<TEntity> items)
        {
            RecordItemCollection = items;
        }

        public void AttachItem(TEntity item)
        {
            RecordItemCollection.Attach(item);
        }

        private void FireEvent(AfterDbProcessHandler handler, TEntity item, ref SqlConnection db, ref SqlTransaction trn)
        {
            if (handler != null)
                handler.Invoke(item, db, trn);
        }

        public virtual bool SaveChanges()
        {
            try
            {
                if (DbConnection.State != System.Data.ConnectionState.Open) DbConnection.Open();
            }
            catch (SqlException ex)
            {
                throw new Exception("Connection Failed", ex);
            }

            var resultSuccess = false;
            foreach (var item in RecordItemCollection.ItemCollection.OrderBy(_ => _.RecordStatus))
            {
                try
                {
                    switch (item.RecordStatus)
                    {
                        case RecordStatus.DeletedRecord:
                            resultSuccess = ExecuteDeleteQuery(item, DbConnection, DbTransaction);
                            if (!resultSuccess) throw new Exception(RaiseError(item));

                            FireEvent(AfterDbDeleteItem, item, ref DbConnection, ref DbTransaction);
                            break;

                        case RecordStatus.ModifiedRecord:
                            resultSuccess = ExecuteUpdateQuery(item, DbConnection, DbTransaction);
                            if (!resultSuccess) throw new Exception(RaiseError(item));

                            FireEvent(AfterDbUpdateItem, item, ref DbConnection, ref DbTransaction);
                            break;

                        case RecordStatus.NewRecord:
                            resultSuccess = ExecuteInsertQuery(item, DbConnection, DbTransaction);
                            if (!resultSuccess) throw new Exception(RaiseError(item));

                            FireEvent(AfterDbInsertItem, item, ref DbConnection, ref DbTransaction);
                            break;
                    }

                    FireEvent(AfterRowChanged, item, ref DbConnection, ref DbTransaction);

                    
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("duplicate"))
                        throw new Exception("Duplicate Record Error!",ex);

                    throw new Exception(RaiseError(item), ex);
                }

            }
            return true;
        }

        private string RaiseError(TEntity item)
        {
            var desc = "";
            var e = ItemError;
            if (e != null) e.Invoke(item, out desc);

            if (String.IsNullOrEmpty(desc)) desc = "Error";
            return desc;
        }

        protected bool ExecuteDeleteQuery(Entity item, SqlConnection db, SqlTransaction trn = null)
        {
            CheckDbConnection();

            var sql = $" Delete from [{item.TableName}] where {item.PrimaryKey} = @Id";
            return db.Execute(sql, item, trn) != 0;
        }

        protected virtual bool ExecuteInsertQuery(TEntity item, SqlConnection db, SqlTransaction trn = null)
        {
            CheckDbConnection();
            try
            {
                item.CreatedBy = Encoder;
                item.ModifiedBy = Encoder;

                item.Id = Convert.ToInt32(DbConnection.Insert(item, DbTransaction));
                if (item.Id == 0) throw new Exception("Error Inserting Data");

                UpdateItemModifiedDates(ref item);
                return true;
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("duplicate"))
                    throw new Exception("Duplicate Record Error!");
                throw;
            }
        }

        protected virtual bool ExecuteUpdateQuery(TEntity item, SqlConnection db, SqlTransaction trn = null)
        {
            CheckDbConnection();
            var result = 0;

            try
            {
                var sql = item.GetUpdateSQLQuery();
                if (!String.IsNullOrEmpty(sql))
                {
                    item.ModifiedBy = Encoder;
                    result = DbConnection.Execute(sql, item, DbTransaction);

                    if (result == 0) throw new Exception("Record NOT saved");
                    UpdateItemModifiedDates(ref item);
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

        protected void UpdateItemModifiedDates(ref TEntity item)
        {
            var sql = string.Format("Select Created, Modified, CreatedBy, ModifiedBy from {0} where {1} = {2}", item.TableName, item.PrimaryKey, item.Id);
            var dynamic = DbConnection.Query(sql, item, transaction: DbTransaction).FirstOrDefault();
        }

        protected void CheckDbConnection()
        {
            if (DbConnection == null) throw new Exception("DbConnection NOT Set. Use SetDbConnection Property");
        }


        public void BeginTransaction()
        {
            if (RecordItemCollection == null) return;
            
            var newItems = RecordItemCollection.ItemCollection.Where(_ => _.RecordStatus == RecordStatus.NewRecord);
            foreach (var item in newItems)
                item.IsNewRecord = true;
        }


        public void CommitTransaction()
        {
            var deletedItems = RecordItemCollection.ItemCollection.Where(o => o.RecordStatus == RecordStatus.DeletedRecord).ToList();
            foreach (var item in deletedItems)
                RecordItemCollection.ItemCollection.Remove(item);

            foreach (var item in RecordItemCollection.ItemCollection)
                item.ClearStatusAndTrackingChanges();
        }


        public void RollBackTransaction()
        {
            if (RecordItemCollection == null) return;
            foreach (var item in RecordItemCollection.ItemCollection)
                if (item.IsNewRecord) item.Id = 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace AiTech.LiteOrm.Database
{

    /// <summary>
    /// 
    /// </summary>
    public class EntityEventArgs : EventArgs
    {
        public Entity ItemData { get; set; }
        public SqlConnection Connection { get; set; }
        public SqlTransaction Transaction { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityCollection"></typeparam>
    public abstract class DataWriter<TEntity, TEntityCollection>
        where TEntity : Entity
        where TEntityCollection : EntityCollection<TEntity>, new()
    {
        public event EventHandler<EntityEventArgs> AfterItemSave;
        public event EventHandler<EntityEventArgs> ErrorOccured;

        protected TEntityCollection _List;

        protected string DataWriterUsername;




        protected virtual void OnAfterItemSave(EntityEventArgs item)
        {
            var handler = AfterItemSave;
            handler?.Invoke(this, item);
        }


        public DataWriter(string username, TEntity item)
        {
            DataWriterUsername = username;

            if (item.Id == 0) item.RowStatus = RecordStatus.NewRecord;
            _List = new TEntityCollection();
            _List.Attach(item);
        }



        public DataWriter(string username, TEntityCollection items)
        {
            DataWriterUsername = username;
            _List = items;
        }




        protected abstract string CreateSqlInsertQuery();
        protected abstract void CreateSqlInsertCommandParameters(SqlCommand cmd, TEntity item);




        protected SqlCommand CreateInsertCommand(SqlConnection db, SqlTransaction trn, TEntity item)
        {
            var insertQuery = CreateSqlInsertQuery();
            var cmd = new SqlCommand(insertQuery, db, trn);

            CreateSqlInsertCommandParameters(cmd, item);

            return cmd;
        }



        protected SqlCommand CreateUpdateCommand(SqlConnection db, SqlTransaction trn, TEntity item)
        {
            var updateQuery = CreateSqlUpdateQuery(item);

            if (string.IsNullOrEmpty(updateQuery)) return null;


            var cmd = new SqlCommand(updateQuery, db, trn);

            CreateSqlUpdateCommandParameters(cmd, item);
            return cmd;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected string CreateSqlUpdateQuery(TEntity item)
        {
            var builder = new SqlUpdateQueryBuilder(item);
            return builder.GetQueryString();
        }



        protected virtual void CreateSqlUpdateCommandParameters(SqlCommand cmd, TEntity item)
        {
            CreateSqlInsertCommandParameters(cmd, item);
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int));
            cmd.Parameters["@Id"].Value = item.Id;
        }



        protected bool ExecuteCommand(SqlCommand cmd, TEntity item, string errorDescription)
        {
            return DatabaseAction.ExecuteCommand<TEntity>(cmd, item, errorDescription);
        }




        protected void UpdateItemRecordInfo(TEntity item, SqlDataReader reader)
        {
            if (!reader.Read()) throw new Exception("Error Inserting New Item");

            item.Id = reader.GetInt32(reader.GetOrdinal("Id"));

            item.Created = reader.GetDateTime(reader.GetOrdinal("Created"));
            item.CreatedBy = reader["CreatedBy"].ToString();
            item.Modified = reader.GetDateTime(reader.GetOrdinal("Modified"));
            item.ModifiedBy = reader["ModifiedBy"].ToString();
        }



        /// <summary>
        /// Write changes to the database
        /// </summary>
        /// <param name="ErrorDescription"></param>
        /// <param name="db"></param>
        /// <param name="trn"></param>
        /// <returns></returns>
        protected bool Write(Expression<Func<TEntity, string>> ErrorDescription, SqlConnection db, SqlTransaction trn)
        {
            var affectedRecords = 0;

            // Delete All Marked Items
            var deletedItems = _List.Items.Where(_ => _.RowStatus == RecordStatus.DeletedRecord);

            var enumerable = deletedItems as IList<TEntity> ?? deletedItems.ToList();
            if (enumerable.Any())
            {
                if (DatabaseAction.ExecuteDeleteQuery<TEntity>(DataWriterUsername, enumerable, db, trn))
                    affectedRecords += enumerable.Count();
            }


            foreach (var item in _List.Items)
            {
                SqlCommand cmd = null;

                string onErrorDescription = ErrorDescription.Compile().Invoke(item);

                switch (item.RowStatus)
                {
                    case RecordStatus.DeletedRecord:
                        continue;

                    case RecordStatus.NewRecord:
                        cmd = CreateInsertCommand(db, trn, item);
                        break;

                    default: // UPDATE
                        cmd = CreateUpdateCommand(db, trn, item);
                        break;
                }

                if (cmd != null)
                    if (ExecuteCommand(cmd, item, onErrorDescription)) affectedRecords++;

                //
                // Save SubClass Here;
                //                               							
                OnAfterItemSave(new EntityEventArgs() { ItemData = item, Connection = db, Transaction = trn });

            }


            return affectedRecords > 0;


        }

    }
}

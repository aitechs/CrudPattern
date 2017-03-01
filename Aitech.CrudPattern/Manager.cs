using System;
using System.Data.SqlClient;
using Dapper.Contrib.Extensions;
using Dapper;
using System.Collections.Generic;

namespace AiTech.CrudPattern
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCollection">Entity Collection</typeparam>
    /// <typeparam name="TEntity">Entity</typeparam>
    public abstract class Manager<TEntity> 
        //where TCollection : EntityCollection<TEntity>
        where TEntity : Entity
    {

        protected EntityCollection<TEntity> ItemCollection;
        
        protected string Encoder { get; set; }

        public abstract bool SaveChanges();        

        public Manager(string encoder)
        {
            Encoder = encoder;
            //ItemCollection = EntityCollection<TEntity>.CreateInstance();
            //if (ItemCollection == null)    throw new Exception("You must Set ItemCollection in Manager = <Manager.Collection>");
        } 

        protected virtual int ExecuteDeleteQuery(SqlConnection db)
        {
            //
            // DELETE ITEMS
            //
            var deletedItems = ItemCollection.GetItemsWithStatus(RecordStatus.DeletedRecord);
            var retVal = 0;
            foreach (var item in deletedItems)
            {
                var table = item.GetTableName();
                var sql =  " Delete from " + table;
                sql += " where id = @Id";

                retVal += db.Execute(sql,item);
            }

            return retVal;
        }

        protected virtual int ExecuteInsertQuery(SqlConnection db)
        {
            try
            {
                var newItems = ItemCollection.GetItemsWithStatus(RecordStatus.NewRecord);
                var result = 0;
                foreach (var item in newItems)
                {
                    //Be Sure to Have  
                    // select cast(Scope_Identity() as int)
                    item.CreatedBy = Encoder;
                    item.ModifiedBy = Encoder;

                    item.Id = Convert.ToInt32(db.Insert(item));
                    if (item.Id == 0) throw new Exception("Error Inserting Data");
                    result++;
                }
                return result;
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("duplicate"))
                    throw new Exception("Duplicate Record Error!");
                throw;
            }
        }

        protected virtual int ExecuteUpdateQuery(SqlConnection db)
        {
            var modifiedItems = ItemCollection.GetItemsWithStatus(RecordStatus.ModifiedRecord);
            try
            {
                var result = 0;
                foreach (var item in modifiedItems)
                {
                    item.ModifiedBy = Encoder;
                    //result =+ (db.Update(item) ? 1 : 0);
                    var sql = item.GetUpdateSQLQuery();
                    if (!String.IsNullOrEmpty(sql))
                        result += db.Execute(sql, item) != 0 ? 1 : 0 ;
                }
                return result;
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("duplicate"))
                    throw new Exception("Duplicate Record Error!");
                throw;
            }
        }

        public void Attach(TEntity item)
        {
            ItemCollection.Attach(item);
        }

        public void AttachRange(IEnumerable<TEntity> items)
        {
            ItemCollection.AttachRange(items);
        }

    }
}

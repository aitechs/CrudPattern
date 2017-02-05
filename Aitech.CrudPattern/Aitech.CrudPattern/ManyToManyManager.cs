using System;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace AiTech.CrudPattern
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Entity Collection</typeparam>
    /// <typeparam name="X">Entity</typeparam>
    public abstract class ManyToManyManager<TCollection, TEntity, TOwner, TSubItem>
        where TCollection: ManyToManyCollection<TOwner, TSubItem>
        where TEntity: Entity
        where TOwner: Entity
        where TSubItem: Entity
    {
        protected TCollection ItemCollection;
        public abstract bool SaveChanges();

        public ManyToManyManager()
        {
            //if (ItemCollection == null)    throw new Exception("You must Set ItemCollection in Manager = <Manager.Collection>");
        }

        protected virtual int ExecuteDeleteQuery(string tableName, SqlConnection db)
        {
            //
            // DELETE ITEMS
            //
            var deletedItems = ItemCollection.GetItemsWithStatus(RecordStatus.DeletedRecord);
            var retVal = 0;
            foreach (var item in deletedItems)
            {
                var sql = string.Format("DELETE FROM [{0}] where Id = @id ", tableName);

                retVal += db.Execute(sql, new { Id = item.Id });
            }

            return retVal;
        }

        protected virtual void ExecuteInsertQuery(string sql, SqlConnection db)
        {
            try
            {
                var newItems = ItemCollection.GetItemsWithStatus(RecordStatus.NewRecord);
                foreach (var item in newItems)
                {
                    //Be Sure to Have  
                    // select cast(Scope_Identity() as int)
                    item.Id = db.Query<int>(sql, item).First();
                    if (item.Id == 0) throw new Exception("Error Inserting Data");
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("duplicate"))
                    throw new Exception("Id Already Exists!");
                throw;
            }
        }

        //protected virtual void ExecuteUpdateQuery(string sql, SqlConnection db)
        //{
        //    var modifiedItems = ItemCollection.GetItems(RecordStatus.ModifiedRecord);
        //    try
        //    {
        //        foreach (var item in modifiedItems)
        //            db.Query<int>(sql, item);                
        //    }
        //    catch (SqlException ex)
        //    {
        //        if (ex.Message.Contains("duplicate"))
        //            throw new Exception("Id Already Exists!");
        //        throw;
        //    }
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace AiTech.CrudPattern
{
    public abstract class DbManager<T> where T:Entity
    {
        protected string Encoder = "";

        protected Dictionary<string, ITransaction> QueryManagerCollection = new Dictionary<string, ITransaction>();

        public DbManager(string encoder)
        {
            Encoder = encoder;
        }

        public virtual void SetItems(EntityCollection<T> items)
        {
            var primary = new QueryManager<T>(Encoder);
                primary.SetItems(items);

            QueryManagerCollection.Add("Primary", primary);
        }

        public virtual void AttachItem(T item)
        {
            var primary = new QueryManager<T>(Encoder);
                primary.AttachItem(item);

            QueryManagerCollection.Add("Primary", primary);
        }

        /// <summary>
        /// After SQL Insert, Get the AutoId and Update all child item foreignKey
        /// </summary>
        /// <typeparam name="TEntity">Child Entity</typeparam>
        /// <param name="items">Entity Collection <TEntity></param>
        /// <param name="actionForEachItem">action to assign the parent Id</param>
        protected void UpdateSubItemParentId<TEntity>(EntityCollection<TEntity> items, Action<TEntity> actionForEachItem) where TEntity : Entity
        {
            foreach (var i in items.Items)
                actionForEachItem(i);
        }

     

        /// <summary>
        /// You can override the savechanges and execute GetItems to get the Main Collection
        /// </summary>
        public virtual bool SaveChanges()
        {
            using (var db = new SqlConnection()) //Connection.DataConnection.CreateConnection)
            {
                try
                {
                    db.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error Opening Connection", ex);
                }


                var trn = BeginTransaction(db);

                try
                {
                    var primary = QueryManagerCollection["Primary"] as QueryManager<T>;
                    primary.SetConnection(db, trn);
                    primary.BeginTransaction();
                    primary.SaveChanges();

                    CommitTransaction(trn);

                    return true;
                }
                catch (Exception ex)
                {
                    RollBackTransaction(trn);
                    throw new Exception("Error Saving Employee", ex);
                }
            }
        }

        #region Transaction Methods
        private SqlTransaction BeginTransaction(SqlConnection db)
        {
            foreach (var keyPair in QueryManagerCollection)
                keyPair.Value.BeginTransaction();

            return db.BeginTransaction();
        }

        private void CommitTransaction(SqlTransaction transaction)
        {
            transaction.Commit();

            //Commit for SubItems
            if (QueryManagerCollection.Count == 0) return;
            foreach (var keyPair in QueryManagerCollection)
                keyPair.Value.CommitTransaction();
        }



        private void RollBackTransaction(SqlTransaction transaction)
        {
            if (transaction != null)
                transaction.Rollback();
          
            //RollBack for SubItems
            if (QueryManagerCollection.Count == 0 ) return;
            foreach (var keyPair in QueryManagerCollection)
                keyPair.Value.RollBackTransaction();

        }

        #endregion    
    }
}

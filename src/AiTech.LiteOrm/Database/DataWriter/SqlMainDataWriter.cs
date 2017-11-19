using System;
using System.Linq.Expressions;

namespace AiTech.LiteOrm.Database
{
    public abstract class SqlMainDataWriter<TEntity, TEntityCollection> : DataWriter<TEntity, TEntityCollection>
        where TEntity : Entity
        where TEntityCollection : EntityCollection<TEntity>, new()
    {

        public event EventHandler<TEntityCollection> AfterCommit;
        public event EventHandler<EntityEventArgs> AfterRollbackChanges;


        public SqlMainDataWriter(string username, TEntity item) : base(username, item) { }

        public SqlMainDataWriter(string username, TEntityCollection items) : base(username, items) { }



        protected virtual void OnAfterCommit(TEntityCollection items)
        {
            var handler = AfterCommit;
            handler?.Invoke(this, items);
        }

        /// <summary>
        /// Commit the Changes  in the class
        /// </summary>
        protected virtual void CommitChanges()
        {
            _List.CommitChanges();

            OnAfterCommit(_List);
        }


        /// <summary>
        /// Rollback Changes in the class
        /// </summary>
        protected virtual void RollbackChanges()
        {
            _List.RollbackChanges();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool SaveChanges()
        {
            return Write(_ => "item");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ErrorDescription"></param>
        /// <returns></returns>
        protected bool Write(Expression<Func<TEntity, string>> ErrorDescription)
        {
            using (var db = Connection.CreateAndOpenConnection())
            {
                var trn = db.BeginTransaction();
                try
                {

                    var success = Write(ErrorDescription, db, trn);

                    trn.Commit();

                    CommitChanges();

                    return success;
                }
                catch (Exception ex)
                {
                    trn.Rollback();
                    RollbackChanges();
                    throw new Exception("Write to Database Error", ex);
                }
            }

        }


    }



}

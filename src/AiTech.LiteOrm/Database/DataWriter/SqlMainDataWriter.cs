﻿using System;
using System.Linq.Expressions;

namespace AiTech.LiteOrm.Database
{
    public abstract class SqlMainDataWriter<TEntity, TEntityCollection> : DataWriter<TEntity, TEntityCollection>
        where TEntity : Entity
        where TEntityCollection : EntityCollection<TEntity>, new()
    {


        public event EventHandler<EntityEventArgs> AfterRollbackChanges;


        public SqlMainDataWriter(string username, TEntity item) : base(username, item) { }

        public SqlMainDataWriter(string username, TEntityCollection items) : base(username, items) { }



        /// <summary>
        /// Commit the Changes  in the class
        /// </summary>
        protected virtual void CommitChanges()
        {
            _List.CommitChanges();
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
            using (var db = Connection.CreateConnection())
            {
                try
                {

                    db.Open();

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Can not establish connection to server", ex);
                }

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

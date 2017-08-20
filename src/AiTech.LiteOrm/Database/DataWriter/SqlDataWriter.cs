using System.Data.SqlClient;

namespace AiTech.LiteOrm.Database
{
    /// <summary>
    /// Class responsible for writing data to database
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TCol"></typeparam>
    public abstract class SqlDataWriter<TEntity, TCol> : DataWriter<TEntity, TCol>
        where TEntity : Entity
        where TCol : EntityCollection<TEntity>, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="username"></param>
        /// <param name="item"></param>
        protected SqlDataWriter(string username, TEntity item) : base(username, item) { }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="username"></param>
        /// <param name="items"></param>
        protected SqlDataWriter(string username, TCol items) : base(username, items) { }


        /// <summary>
        /// Save Data specifying connection and transaction
        /// </summary>
        /// <param name="db"></param>
        /// <param name="trn"></param>
        /// <returns></returns>
        public abstract bool SaveChanges(SqlConnection db, SqlTransaction trn);


    }
}

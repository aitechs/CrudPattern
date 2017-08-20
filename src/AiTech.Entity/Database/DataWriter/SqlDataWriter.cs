using System.Data.SqlClient;

namespace AiTech.LiteOrm.Database
{
    public abstract class SqlDataWriter<TEntity, TCol> : DataWriter<TEntity, TCol>
        where TEntity : Entity
        where TCol : EntityCollection<TEntity>, new()
    {

        public SqlDataWriter(string username, TEntity item) : base(username, item) { }
        public SqlDataWriter(string username, TCol items) : base(username, items) { }

        public abstract bool SaveChanges(SqlConnection db, SqlTransaction trn);


    }
}

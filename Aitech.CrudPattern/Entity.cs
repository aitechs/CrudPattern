using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using AiTech.CrudPattern;

namespace AiTech.CrudPattern
{
    public abstract class Entity
    {
        public int Id { get; set; }

        [Write(false)]
        public string RowId { get; set; }

        [Write(false)]
        public RecordStatus RowStatus { get; set; }

        [Computed]
        protected IDictionary<string, object> Changes { get; private set; }

        [Write(false)]
        protected internal bool IsNewRecord { get; set; }


        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        [Write(false)]
        public DateTime Created { get; set; }
        [Write(false)]
        public DateTime Modified { get; set; }


        public Entity()
        {
            RowId = Guid.NewGuid().ToString();
            RowStatus = RecordStatus.NoChanges;

            Changes = new Dictionary<string, object>();

            TableName = GetTableName();
            PrimaryKey = GetPrimaryKey();
        }

        protected void OnChanged(dynamic value, [CallerMemberName] string caller = "")
        {
            if (Id != 0)
            {
                RowStatus = RecordStatus.ModifiedRecord;
            }

            if (Changes.ContainsKey(caller))
                Changes[caller] = value;
            else
                Changes.Add(caller, value);
        }


        public void ClearTrackingChanges()
        {
            Changes.Clear();
        }

        public void ClearStatusAndTrackingChanges()
        {
            RowStatus = RecordStatus.NoChanges;
            Changes.Clear();
        }

        internal string GetUpdateSQLQuery()
        {
            if (Changes.Count() == 0) return "";

            var tableName = GetTableName();

            var sql = "UPDATE [" + tableName + "] SET ";

            foreach (var i in Changes)
                sql += i.Key + " = @" + i.Key + ",";

            //Add the modified
            sql += " Modified = GetDate()";
            sql += $" WHERE {PrimaryKey} = @Id";

            return sql;
        }


        protected internal string TableName;
        /// <summary>
        /// Get The Table Name from Entity
        /// </summary>
        /// <returns></returns>
        internal string GetTableName()
        {
            var tableNameAttribute = this.GetType().GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            if (tableNameAttribute == null)
                return this.GetType().Name;

            return tableNameAttribute.Name;
        }


        protected internal string PrimaryKey;
        private string GetPrimaryKey()
        {
            var keyAttribute = this.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(KeyAttribute))).FirstOrDefault();
            if (keyAttribute == null)
                return "Id";

            return keyAttribute.Name;
        }

        internal protected virtual void CopyTo<TEntity>(ref TEntity destination) where TEntity : Entity
        {
            destination = (TEntity)MemberwiseClone();
        }

    }
}

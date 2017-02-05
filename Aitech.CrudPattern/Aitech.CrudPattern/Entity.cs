using System;
using Dapper.Contrib.Extensions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

namespace AiTech.CrudPattern
{
    public abstract class Entity
    {
        public int Id { get; set; }

        [Write(false)]
        public string Token { get; set; }

        [Write(false)]
        public RecordStatus RecordStatus { get; set; }

        [Computed]
        protected IDictionary<string, object> Changes { get; private set; }

        
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        [Write(false)]
        public DateTime Created { get; set; }
        [Write(false)]
        public DateTime Modified { get; set; }


        public Entity()
        {
            Token = Guid.NewGuid().ToString();
            RecordStatus = RecordStatus.NoChanges;

            Changes = new Dictionary<string, object>();
        }

        [Obsolete("use OnChanged(Values)")]
        protected virtual void OnChanged()
        {
            if (Id != 0) RecordStatus = RecordStatus.ModifiedRecord;
        }

        
        protected void OnChanged(dynamic value, [CallerMemberName] string caller = "")
        {
            if (Id != 0) RecordStatus = RecordStatus.ModifiedRecord;

            if (Changes.ContainsKey(caller))
                Changes[caller] = value;
            else
                Changes.Add(caller, value);
        }


        public void ClearTrackingChanges()
        {
            Changes = new Dictionary<string, object>();
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
            sql += " WHERE Id = @Id";

            return sql;
        }


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

    }
}

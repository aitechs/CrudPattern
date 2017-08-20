using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AiTech.LiteOrm
{
    public abstract class Entity : IRecordInfo, ITrackableObject
    {
        public int Id { get; set; }

        [Write(false)]
        public string RowId { get; set; }

        [Write(false)]
        public RecordStatus RowStatus { get; set; }

        [Write(false)]
        protected internal bool IsNewRecord { get; set; }


        #region Record Info Fields

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        [Write(false)]
        public DateTime Created { get; set; }

        [Write(false)]
        public DateTime Modified { get; set; }

        #endregion

        public Entity()
        {
            RowId = Guid.NewGuid().ToString();

            RowStatus = RecordStatus.NoChanges;

            //Changes = new Dictionary<string, object>();

            TableName = GetTableName();
            PrimaryKey = GetPrimaryKey();

            Created = new DateTime(1920, 1, 1);
            Modified = new DateTime(1920, 1, 1);

            OriginalValues = new Dictionary<string, object>();
        }


        /// <summary>
        /// Name of Table on Database
        /// </summary>
        protected internal string TableName;


        /// <summary>
        /// Get The Table Name from Entity Attribute
        /// </summary>
        /// <returns></returns>
        private string GetTableName()
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



        public Dictionary<string, object> OriginalValues { get; protected set; }
        public abstract void StartTrackingChanges();
        public abstract Dictionary<string, object> GetChangedValues();

    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace AiTech.LiteOrm
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntityName">Name of the Entity inside Collection</typeparam>
    [Serializable]
    public abstract class EntityCollection<TEntityName> where TEntityName : Entity
    {
        protected internal ICollection<TEntityName> ItemCollection;

        public IEnumerable<TEntityName> Items { get; set; }

        public EntityCollection()
        {
            ItemCollection = new List<TEntityName>();
            Items = ItemCollection.AsEnumerable(); //.Where(o => o.RowStatus != RecordStatus.DeletedRecord);
        }


        public virtual void Add(TEntityName item)
        {
            var itemFound = ItemCollection.FirstOrDefault(x => x.RowId == item.RowId);
            if (itemFound != null) throw new Exception("Record Already Exists");

            item.RowStatus = RecordStatus.NewRecord;
            ItemCollection.Add(item);
        }

        public virtual void AddRange(IEnumerable<TEntityName> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public virtual void Attach(TEntityName item)
        {
            ItemCollection.Add(item);
        }

        public virtual void AttachRange(IEnumerable<TEntityName> items)
        {
            foreach (var item in items)
                Attach(item);
        }

        public virtual void Remove(TEntityName item)
        {
            //Check if it has an Id. If it has it means, it is from DB.
            if (item.Id == 0)
            {
                ItemCollection.Remove(item);
                return;
            }

            //Find the User
            var foundItem = ItemCollection.FirstOrDefault(o => o.Id == item.Id || o.RowId == item.RowId);
            if (foundItem == null) throw new Exception("Record Not Found");

            foundItem.RowStatus = RecordStatus.DeletedRecord;
        }

        public virtual void RemoveAll()
        {
            for (var i = ItemCollection.Count - 1; i >= 0; i--)
            {
                var item = ItemCollection.ElementAt(i);
                if (item.Id == 0)
                {
                    ItemCollection.Remove(item);
                    continue;
                }

                item.RowStatus = RecordStatus.DeletedRecord;
            }

        }

        public virtual void Remove(int index)
        {
            var item = ItemCollection.ElementAt(index);
            if (item == null) return;
            Remove(item);
        }

        public void CommitChanges()
        {
            var deletedItems = ItemCollection.Where(o => o.RowStatus == RecordStatus.DeletedRecord).ToList();
            foreach (var item in deletedItems)
                ItemCollection.Remove(item);

            foreach (var item in ItemCollection)
            {
                item.RowStatus = RecordStatus.NoChanges;
                item.StartTrackingChanges();
            }
        }

        public void RollbackChanges()
        {
            foreach (var item in ItemCollection)
            {
                if (item.RowStatus == RecordStatus.NewRecord) item.Id = 0;
            }
        }


        /// <summary>
        /// Load items to collection setting each item rowstatus = NoChanges and StartTrackingChanges
        /// </summary>
        /// <param name="items"></param>
        protected internal void LoadItemsWith(IEnumerable<TEntityName> items)
        {
            ItemCollection.Clear();
            foreach (var item in items)
            {
                item.RowStatus = RecordStatus.NoChanges;
                item.StartTrackingChanges();
                ItemCollection.Add(item);
            }
        }


        /// <summary>
        /// Switch to check if previously loaded from Database
        /// </summary>
        public bool HasReadFromDb { get; protected set; }


        public IEnumerable<TEntityName> GetDirtyItems()
        {
            return ItemCollection.Where(r => r.RowStatus != RecordStatus.NoChanges);
        }


    }
}

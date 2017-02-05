using System;
using System.Collections.Generic;
using System.Linq;

namespace AiTech.CrudPattern
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntityName">Name of the Entity inside Collection</typeparam>
    public abstract class EntityCollection<TEntityName> where TEntityName : Entity
    {
        //internal abstract void LoadItems();

        private ICollection<TEntityName> ItemCollection;

        public IEnumerable<TEntityName> Items { get; set; }


        public EntityCollection()
        {
            ItemCollection = new List<TEntityName>();
            Items = ItemCollection.Where(o => o.RecordStatus != RecordStatus.DeletedRecord);
        }


        public virtual void Add(TEntityName item)
        {
            var itemFound = ItemCollection.FirstOrDefault(x => x.Token == item.Token);
            if (itemFound != null) throw new Exception("Record Already Exists");

            item.RecordStatus = RecordStatus.NewRecord;
            ItemCollection.Add(item);
        }

        public virtual void AddRange(IEnumerable<TEntityName> items)
        {
            foreach (var item in items)
                Add(item);
        }

        public virtual void Attach(TEntityName item)
        {
            //if (item.Id != 0) item.RecordStatus = RecordStatus.ModifiedRecord;
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
            var foundItem = ItemCollection.FirstOrDefault(o => o.Id == item.Id  ||   o.Token == item.Token);
            if (foundItem == null) throw new Exception("Record Not Found");
            
            foundItem.RecordStatus = RecordStatus.DeletedRecord;
        }

        public virtual void Remove(int index)
        {
            var item = ItemCollection.ElementAt(index);
            if (item == null) return;
            Remove(item);           
        }

        internal IEnumerable<TEntityName> GetItemsWithStatus(RecordStatus status)
        {
            return ItemCollection.Where(r => r.RecordStatus == status);
        }

        [Obsolete("Optional")]
        public virtual bool LoadItemsFromDb()
        {
            return false;
        }
            

        /// <summary>
        /// Call this Method right after LoadItemsFromDb to Transfer data to ItemCollection
        /// </summary>
        /// <param name="items"></param>
        internal void LoadItems(IEnumerable<TEntityName> items )
        {
            ItemCollection.Clear();
            foreach (var item in items)
            {
                item.RecordStatus = RecordStatus.NoChanges;
                item.ClearTrackingChanges();
                ItemCollection.Add(item);
            }
        }

        /// <summary>
        /// Clear Status of All Items.
        /// Call Item.ClearTrackingChanges() 
        ///      RecordStatus = NoChanges
        /// </summary>
        public void ClearTrackingChanges()
        {
            foreach (var item in ItemCollection)
            {
                item.ClearTrackingChanges();
                item.RecordStatus = RecordStatus.NoChanges;
            }
        }

        public IEnumerable<TEntityName> GetItemsToBeSaved()
        {
            return ItemCollection.Where(r => r.RecordStatus != RecordStatus.NoChanges);
        }

    }
}

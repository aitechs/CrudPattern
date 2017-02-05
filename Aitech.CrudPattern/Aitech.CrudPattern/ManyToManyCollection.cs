using System;
using System.Collections.Generic;
using System.Linq;

namespace AiTech.CrudPattern
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Collection of What Entity</typeparam>
    /// <typeparam name="X">Owner Class</typeparam>
    /// <typeparam name="Y">Sub Item Class</typeparam>
    public abstract class ManyToManyCollection<TOwner, TSubItem>
        where TOwner : Entity 
        where TSubItem: Entity
    {
        internal ICollection<ManyToManyEntity<TOwner,TSubItem>> ItemCollection;
        protected TOwner _Owner;

        public IEnumerable<TSubItem> Items { get; set; }

        public ManyToManyCollection(TOwner owner)
        {
            ItemCollection = new List<ManyToManyEntity<TOwner,TSubItem>>();
            _Owner = owner;

            Items = (IEnumerable<TSubItem> )ItemCollection.Where(o => o.RecordStatus != RecordStatus.DeletedRecord)
                        .Select(o=>o.SubItem);
        }

        public void Add(TSubItem item)
        {
            var itemFound = ItemCollection.FirstOrDefault(o => o.Token == item.Token);
            if(itemFound !=  null) throw new Exception("Item Exists");

            ItemCollection.Add( new ManyToManyEntity<TOwner,TSubItem>()
            {                
                Owner = _Owner,
                SubItem = item,
                RecordStatus = RecordStatus.NewRecord
            });
        }

        public void Remove(TSubItem item)
        {
            var itemFound = ItemCollection.FirstOrDefault(x => x.SubItem.Token == item.Token);
            if (itemFound == null) throw new Exception("Item Does Not Exists");

            if (itemFound.Id == 0)
                ItemCollection.Remove(itemFound);
            else 
                itemFound.RecordStatus = RecordStatus.DeletedRecord;
        }

        internal IEnumerable<ManyToManyEntity<TOwner, TSubItem>> GetItemsWithStatus(RecordStatus status)
        {
            return (IEnumerable<ManyToManyEntity<TOwner, TSubItem>>) ItemCollection.Where(r => r.RecordStatus == status);
        }
    }
}

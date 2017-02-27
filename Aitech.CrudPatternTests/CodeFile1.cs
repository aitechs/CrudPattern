using AiTech.CrudPattern;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AiTech.CrudPattern.Tests
{
    public class SampleEntity : Entity
    {
        public string Name { get; set; }
    }

    public class SampleEntityCollection : EntityCollection<SampleEntity>
    {
        public SampleEntityCollection()
        {
            
        }
    }

    public class SampleEntityManager : Manager<SampleEntity>
    {
        public SampleEntityManager(string encoder) : base(encoder)
        {
            ItemCollection = new SampleEntityCollection();
        }

        public void AttachRange(IEnumerable<SampleEntity> items)
        {
            Assert.IsNotNull(ItemCollection, "ItemCollection is NULL");
            ItemCollection.AttachRange(items);
        }

        public void Attach(SampleEntity item)
        {
            Assert.IsNotNull(ItemCollection);

            ItemCollection.Attach(item);
        }

        public override bool SaveChanges()
        {
            var inserted = ItemCollection.GetItemsWithStatus(RecordStatus.NewRecord);
            Assert.AreEqual(ItemCollection.Items.ElementAt(0).Name, inserted.ElementAt(0).Name);

            return true;
        }
    }
}
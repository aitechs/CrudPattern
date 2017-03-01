using AiTech.CrudPattern;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AiTech.CrudPattern.Tests
{
    public class SampleEntity : Entity
    {
        public string Name { get; set; }

        public void CopyTo(ref SampleEntity destination)
        {
            base.CopyTo<SampleEntity>(ref destination);

            destination.Histories = new HistoryCollection();
            this.Histories.CopyTo<HistoryCollection>(destination.Histories);
        }

        
        public HistoryCollection Histories { get; set; }

        public SampleEntity()
        {
            Histories = new HistoryCollection();
        }
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

       
        public override bool SaveChanges()
        {
            var inserted = ItemCollection.GetItemsWithStatus(RecordStatus.NewRecord);
            Assert.AreEqual(ItemCollection.Items.ElementAt(0).Name, inserted.ElementAt(0).Name);

            return true;
        }
    }


    public class History:Entity
    {
        public string Batch { get; set; }

        public void CopyTo(ref History destination)
        {
            destination = (History)MemberwiseClone();
        }
    }

    public class HistoryCollection : EntityCollection<History>
    {
        
    }
}
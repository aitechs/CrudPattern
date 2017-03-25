using Microsoft.VisualStudio.TestTools.UnitTesting;
using AiTech.CrudPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiTech.CrudPattern.Tests
{
    [TestClass()]
    public class ManagerTests
    {
        [TestMethod()]
        public void TestClone()
        {
            var entity = new SampleEntity();
            
            entity.Name = "Name1";
            entity.Id = 123;
            entity.RecordStatus = Aitech.CrudPattern.RecordStatus.NewRecord;
            entity.Modified = new DateTime(2017, 01, 01);
            entity.Created = new DateTime(2010, 10, 10);
            entity.CreatedBy = "New Encoder";

            var history = new History();
            history.Batch = "2016-2017";

            entity.Histories.Add(history);

            Assert.AreEqual(1, entity.Histories.Items.Count());

            var clone = new SampleEntity();

            Assert.AreNotSame(clone, entity);


            Assert.AreNotSame(clone.Histories, entity.Histories,"The same");
            var originalHistories = entity.Histories;

            entity.CopyTo(ref clone);

            Assert.AreSame(originalHistories, entity.Histories);

            Assert.AreNotSame(entity, clone);
            Assert.AreEqual(entity.Id, clone.Id, "NOt Equal ID");
            Assert.AreEqual(entity.Modified, clone.Modified, "NOT EQUAL TOken");
            Assert.AreEqual(entity.Token, clone.Token, "NOT EQUAL TOken");

            Assert.AreEqual(1, clone.Histories.Items.Count(),"Not same size");

            Assert.AreNotSame(clone.Histories, entity.Histories, "Histories are the same");


            Assert.AreNotSame(clone.Histories.Items.ElementAt(0), entity.Histories.Items.ElementAt(0));

            var node1 = entity.Histories.Items.ElementAt(0);
            var clone1 = clone.Histories.Items.ElementAt(0);
            // Assert.AreEqual(clone.Histories.Items.ElementAt(0), entity.Histories.Items.ElementAt(0),"Error");

            Assert.AreEqual(node1.Token, clone1.Token);
            Assert.AreEqual(node1.RecordStatus, clone1.RecordStatus);
        }

        [TestMethod()]
        public void ManagerTest()
        {
            var entity = new SampleEntity();

            Assert.IsNotNull(entity, "Null Entity");

            entity.Name = "MyName";

            Assert.AreEqual("MyName", entity.Name);

            var collection = new SampleEntityCollection();

            Assert.IsNotNull(collection, "Collection is Null");

            collection.Add(entity);


            Assert.AreEqual(1, collection.Items.Count());

            var manager = new SampleEntityManager("Encoder");

            Assert.IsNotNull(manager);

           
            manager.AttachRange(collection.GetDirtyItems());

            manager.SaveChanges();
        }
        
    }


}
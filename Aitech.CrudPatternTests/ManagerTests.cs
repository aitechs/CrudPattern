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
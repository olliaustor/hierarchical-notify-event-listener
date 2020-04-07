using System;
using System.Collections.Generic;
using System.ComponentModel;
using HierarchicalEventListener;
using HierarchicalEventListenerAppTests.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HierarchicalEventListenerAppTests
{
/*
    [TestClass]
    public class HierarchicalEventListenerCollectionTests
    {
        private IList<ChangingClass> collection;
        private IHierarchicalEventListener listener;

        [TestInitialize]
        public void Initialize()
        {
            listener = new HierarchicalEventListener.HierarchicalEventListener();
            collection = new List<ChangingClass>();
        }

        [TestMethod]
        public void AddItemToIList_CallsPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            int notificationCount = 0;
            listener.PropertyChanged += (sender, args) => { receivedArgs = args; notificationCount++; };
            var item = new ChangingClass(1);
            listener.Attach(item);
            var itemElem1 = new ChangingClass(11);
            item.Elements.Add(itemElem1);

            Assert.IsNotNull(receivedArgs);
            // Assert.AreEqual(2, notificationCount, "number of notifications"); // would be great if we could reduce this to 1

            receivedArgs = null; notificationCount = 0;
            item.Elements[0].Name = "huhu";

            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(1, notificationCount, "number of notifications");
        }
        [TestMethod]
        public void ChangeAddedItemInIList_CallsPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            int notificationCount = 0;
            listener.PropertyChanged += (sender, args) => { receivedArgs = args; notificationCount++; };

            var item = new ChangingClass(1);
            var itemElem1 = new ChangingClass(11);
            item.Elements.Add(itemElem1);
            listener.Attach(item);

            receivedArgs = null; notificationCount = 0;

            item.Elements[0].Name = "huhu";

            Assert.IsNotNull(receivedArgs);
            Assert.AreEqual(1, notificationCount, "number of notifications");
        }
    }
*/
}

using System;
using System.ComponentModel;
using HierarchicalEventListener;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HierarchicalEventListenerAppTests
{
    [TestClass]
    public class HierarchicalEventListenerINotifyPropertyChangedTests
    {
        private IHierarchicalEventListener listener;

        [TestInitialize]
        public void Initialize()
        {
            listener = new HierarchicalEventListener.HierarchicalEventListener();
        }

        #region flat string
        [TestMethod]
        public void FlatStringChangeCallsPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            listener.Attach(item);

            item.Name = "Something";

            Assert.IsNotNull(receivedArgs);
        }

        [TestMethod]
        public void FlatStringChangeCallsPropertyChanged_WithValidSender()
        {
            object receivedSender = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedSender = sender;
            };
            var item = new ChangingClass(1);
            listener.Attach(item);

            item.Name = "Something";

            Assert.AreEqual(item, receivedSender);
        }

        [TestMethod]
        public void FlatStringChangeCallsPropertyChanged_WithValidPropertyName()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            listener.Attach(item);

            item.Name = "Something";

            Assert.AreEqual("Name", receivedArgs.PropertyName);
        }
        #endregion

        #region recursive string with preassigned subobject
        [TestMethod]
        public void RecursiveStringChangeCallsPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1)
            {
                SubObject = new ChangingClass(2)
            };
            listener.Attach(item);

            item.SubObject.Name = "Something";

            Assert.IsNotNull(receivedArgs);
        }

        [TestMethod]
        public void RecursiveStringChangeCallsPropertyChanged_WithValidSender()
        {
            object receivedSender = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedSender = sender;
            };
            var item = new ChangingClass(1)
            {
                SubObject = new ChangingClass(2)
            };
            listener.Attach(item);

            item.SubObject.Name = "Something";

            Assert.AreEqual(item.SubObject, receivedSender);
        }

        [TestMethod]
        public void RecursiveStringChangeCallsPropertyChanged_WithValidPropertyName()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1)
            {
                SubObject = new ChangingClass(2)
            };
            listener.Attach(item);

            item.SubObject.Name = "Something";

            Assert.AreEqual("Name", receivedArgs.PropertyName);
        }
        #endregion

        #region recursive string with later assigned subobject
        [TestMethod]
        public void RecursiveWitLaterAssignedSubObjectStringChangeCallsPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            listener.Attach(item);
            item.SubObject = new ChangingClass(2);

            item.SubObject.Name = "Something";

            Assert.IsNotNull(receivedArgs);
        }

        [TestMethod]
        public void RecursiveWitLaterAssignedSubObjectStringChangeCallsPropertyChanged_WithValidSender()
        {
            object receivedSender = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedSender = sender;
            };
            var item = new ChangingClass(1);
            listener.Attach(item);
            item.SubObject = new ChangingClass(2);

            item.SubObject.Name = "Something";

            Assert.AreEqual(item.SubObject, receivedSender);
        }

        [TestMethod]
        public void RecursiveWitLaterAssignedSubObjectStringChangeCallsPropertyChanged_WithValidPropertyName()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            listener.Attach(item);
            item.SubObject = new ChangingClass(2);

            item.SubObject.Name = "Something";

            Assert.AreEqual("Name", receivedArgs.PropertyName);
        }
        #endregion

        #region recursive string with detached subobject
        [TestMethod]
        public void DetachedSubObject_DoesNotCallPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            var subObject = new ChangingClass(2);
            listener.Attach(item);
            item.SubObject = subObject;

            // Now detach again and change string property
            item.SubObject = null;
            receivedArgs = null;
            subObject.Name = "Something";

            // And changing anything in a detached object should not trigger
            // the event of the listener
            Assert.IsNull(receivedArgs);
        }

        [TestMethod]
        public void DetachedSubObjectWithSubObject_DoesNotCallPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            var subObject = new ChangingClass(2);
            subObject.SubObject = new ChangingClass(3);
            listener.Attach(item);
            item.SubObject = subObject;

            // Now detach again and change string property
            item.SubObject = null;
            receivedArgs = null;
            subObject.SubObject.Name = "Something";

            // And changing anything in a detached object should not trigger
            // the event of the listener
            Assert.IsNull(receivedArgs);
        }
        #endregion

        #region
        [TestMethod]
        public void ReplacedSubObject_DoesNotCallPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            var subObject = new ChangingClass(2);
            var subObjectNew = new ChangingClass(3);
            listener.Attach(item);
            item.SubObject = subObject;

            // Now detach again and change string property
            item.SubObject = subObjectNew;
            receivedArgs = null;
            subObject.Name = "Something";

            // And changing anything in a detached object should not trigger
            // the event of the listener
            Assert.IsNull(receivedArgs);
        }

        [TestMethod]
        public void ReplacingSubObject_CallPropertyChanged()
        {
            PropertyChangedEventArgs receivedArgs = null;
            listener.PropertyChanged += (sender, args) =>
            {
                receivedArgs = args;
            };
            var item = new ChangingClass(1);
            var subObject = new ChangingClass(2);
            var subObjectNew = new ChangingClass(3);
            listener.Attach(item);
            item.SubObject = subObject;

            // Now detach again and change string property
            item.SubObject = subObjectNew;
            receivedArgs = null;
            subObjectNew.Name = "Something";

            // And changing anything in the replacing object should trigger
            // the event of the listener
            Assert.IsNotNull(receivedArgs);
        }
        #endregion
    }

    public class ChangingClass : INotifyPropertyChanged
    {
        private string name;
        private ChangingClass subObject;

        public ChangingClass(int id)
        {
            Id = id;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; private set; }

        public string Name 
        {             
            get
            {
                return name;
            }

            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
                
        }

        public ChangingClass SubObject
        {
            get
            {
                return subObject;
            }

            set
            {
                subObject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubObject)));
            }
        }
    }
}

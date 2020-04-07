namespace HierarchicalEventListener
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    public interface IHierarchicalEventListener : INotifyPropertyChanged
    {
        void Attach(INotifyPropertyChanged notifyPropertyChanged);
        void Detach(INotifyPropertyChanged notifyPropertyChanged);

        void Attach(INotifyCollectionChanged notifyCollectionChanged);
        void Detach(INotifyCollectionChanged notifyCollectionChanged);
    }

    public class HierarchicalEventListener : IHierarchicalEventListener
    {
        private readonly IDictionary<INotifyPropertyChanged, KnownNotifyObject> knownObjects = new Dictionary<INotifyPropertyChanged, KnownNotifyObject>();
        private readonly IList<IEnumerable> knownCollections = new List<IEnumerable>();

        public HierarchicalEventListener()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region INotifyPropertyChanged        
        public void Attach(INotifyPropertyChanged notifyPropertyChanged)
        {
            Attach(null, null, notifyPropertyChanged);
        }

        public void Detach(INotifyPropertyChanged notifyPropertyChanged)
        {
            if (!knownObjects.TryGetValue(notifyPropertyChanged, out KnownNotifyObject knownObject))
            {
                return;
            }

            knownObjects.Remove(notifyPropertyChanged);
            notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;

            foreach (var property in knownObject.Properties.Values)
            {
                Detach(property.Value as INotifyPropertyChanged);
                Detach(property.Value as INotifyCollectionChanged);
            }
        }

        private void Attach(object parent, string propertyName, INotifyPropertyChanged notifyPropertyChanged)
        {
            if (notifyPropertyChanged == null)
            {
                return;
            }

            if (parent != null & propertyName != null && 
                knownObjects.TryGetValue(parent as INotifyPropertyChanged, out KnownNotifyObject knownParent))
            {
                knownParent.Properties.Add(propertyName, new KnownNotifyProperty
                {
                    PropertyName = propertyName,
                    Value = notifyPropertyChanged
                });
            }

            // Try to find object in internal list
            if (!knownObjects.TryGetValue(notifyPropertyChanged, out KnownNotifyObject knownObject))
            {
                // If object is not known yet -> Create and add to internal list
                knownObject = new KnownNotifyObject
                {
                    Owner = notifyPropertyChanged,
                    Parent = parent
                };
                knownObjects.Add(notifyPropertyChanged, knownObject);

                // Do not forget to subscribe to property changed event
                notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
            }         

            // Check recursively for properties to watch and add them too
            foreach (var item in GetPropertiesOf<INotifyPropertyChanged>(notifyPropertyChanged))
            {
                Attach(notifyPropertyChanged, item.Item1, item.Item2);
            }

            // Check recursively for collections to watch and add them too
            foreach (var item in GetPropertiesOf<INotifyCollectionChanged>(notifyPropertyChanged))
            {
                //Attach(notifyPropertyChanged, item.Item1, item.Item2);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var property = sender.GetType().GetProperty(args.PropertyName);
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(property.PropertyType))                
            {
                var value = property.GetValue(sender) as INotifyPropertyChanged;

                if (knownObjects.TryGetValue(sender as INotifyPropertyChanged, out KnownNotifyObject knownObject) &&
                    knownObject.Properties.TryGetValue(args.PropertyName, out KnownNotifyProperty knownProperty) &&
                    value != knownProperty.Value)
                {
                    knownObject.Properties.Remove(args.PropertyName);
                    Detach(knownProperty.Value as INotifyPropertyChanged);
                }
                
                Attach(sender, args.PropertyName, value);
            }

            PropertyChanged?.Invoke(sender, args);
        }
        #endregion

        #region INotifyCollectionChanged        
        public void Attach(INotifyCollectionChanged collection)
        {
            Attach(null, null, collection);
        }

        public void Detach(INotifyCollectionChanged notifyCollectionChanged)
        {
            return;

            if (!(notifyCollectionChanged is IEnumerable collection))
            {
                return;
            }

            knownCollections.Remove(collection);
            notifyCollectionChanged.CollectionChanged += OnCollectionChanged;

            foreach (var item in collection)
            {
                Detach(item as INotifyCollectionChanged);
                Detach(item as INotifyPropertyChanged);
            }
        }

        private void Attach(object parent, string propertyName, INotifyCollectionChanged notifyCollectionChanged)
        {
            return;

            if (!(notifyCollectionChanged is IEnumerable collection))
            {
                return;
            }            

            if (parent != null & propertyName != null &&
                knownObjects.TryGetValue(parent as INotifyPropertyChanged, out KnownNotifyObject knownParent))
            {
                knownParent.Properties.Add(propertyName, new KnownNotifyProperty
                {
                    PropertyName = propertyName,
                    Value = collection
                });
            }

            // Try to find object in internal list
            if (!knownCollections.Contains(collection))
            {
                knownCollections.Add(collection);

                // Do not forget to subscribe to collection changed event
                notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
            }

            foreach (var item in collection)
            {
                Attach(item as INotifyCollectionChanged);
                Attach(item as INotifyPropertyChanged);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        Attach(item as INotifyCollectionChanged);
                        Attach(item as INotifyPropertyChanged);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        Detach(item as INotifyCollectionChanged);
                        Detach(item as INotifyPropertyChanged);
                    }
                    break;
            }
        }
        #endregion

        private IEnumerable<Tuple<string,T>> GetPropertiesOf<T>(object objectToExamine) where T : class
        {
            return objectToExamine.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof(T).IsAssignableFrom(p.PropertyType))
                .Select(p => new Tuple<string, T>(p.Name, p.GetValue(objectToExamine) as T))
                .Where(p => p.Item2 != null);
        }
    }

    public class KnownNotifyObject
    {
        public object Parent { get; set; }
        public object Owner { get; set; }

        public IDictionary<string, KnownNotifyProperty> Properties = new Dictionary<string, KnownNotifyProperty>();
    }

    public class KnownNotifyProperty
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }
}

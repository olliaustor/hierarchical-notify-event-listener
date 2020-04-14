namespace HierarchicalEventListener {
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.ComponentModel;
  using System.Linq;
  using System.Reflection;

  public interface IHierarchicalEventListener : INotifyPropertyChanged {
    void Attach(INotifyPropertyChanged notifyPropertyChanged);
    void Detach(INotifyPropertyChanged notifyPropertyChanged);

    void Attach(INotifyCollectionChanged notifyCollectionChanged);
    void Detach(INotifyCollectionChanged notifyCollectionChanged);
  }

  public class HierarchicalEventListener : IHierarchicalEventListener {
    readonly IDictionary<INotifyPropertyChanged, KnownNotifyObject> knownObjects = new Dictionary<INotifyPropertyChanged, KnownNotifyObject>();
    readonly IList<IEnumerable> knownCollections = new List<IEnumerable>();

    public HierarchicalEventListener() { }

    public event PropertyChangedEventHandler PropertyChanged;

    #region INotifyPropertyChanged        
    public void Attach(INotifyPropertyChanged notifyPropertyChanged) => Attach(null, null, notifyPropertyChanged);

    public void Detach(INotifyPropertyChanged notifyPropertyChanged) {
      if (!knownObjects.TryGetValue(notifyPropertyChanged, out KnownNotifyObject knownObject)) {
        return;
      }

      knownObjects.Remove(notifyPropertyChanged);
      notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;

      foreach (var property in knownObject.Properties.Values) {
        Detach(property.Value as INotifyPropertyChanged);
        Detach(property.Value as INotifyCollectionChanged);
      }
    }

    void Attach(object parent, string propertyName, INotifyPropertyChanged notifyPropertyChanged) {
      if (notifyPropertyChanged == null) {
        return;
      }

      if (parent != null & propertyName != null &&
          knownObjects.TryGetValue(parent as INotifyPropertyChanged, out KnownNotifyObject knownParent)) {
        if (knownParent.Properties.ContainsKey(propertyName)) {
          return;
        }

        knownParent.Properties.Add(propertyName, new KnownNotifyProperty {
          PropertyName = propertyName,
          Value = notifyPropertyChanged
        });
      }

      // Try to find object in internal list
      if (!knownObjects.TryGetValue(notifyPropertyChanged, out KnownNotifyObject knownObject)) {
        // If object is not known yet -> Create and add to internal list
        knownObject = new KnownNotifyObject {
          Owner = notifyPropertyChanged,
          Parent = parent
        };
        knownObjects.Add(notifyPropertyChanged, knownObject);

        // Do not forget to subscribe to property changed event
        notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
      }

      // Check recursively for collections to watch and add them too
      foreach (var item in GetPropertiesOf<INotifyCollectionChanged>(notifyPropertyChanged)) {
        Attach(notifyPropertyChanged, item.Item1, item.Item2);
      }

      // Check recursively for properties to watch and add them too
      foreach (var item in GetPropertiesOf<INotifyPropertyChanged>(notifyPropertyChanged)) {
        Attach(notifyPropertyChanged, item.Item1, item.Item2);
      }
    }

    void OnPropertyChanged(object sender, PropertyChangedEventArgs args) {
      if (args.PropertyName.Contains("[")) {
        return; // no indexer properties allowed
      }

      var property = sender.GetType().GetProperty(args.PropertyName);
      var value = property.GetValue(sender);
      if (knownObjects.TryGetValue(sender as INotifyPropertyChanged, out KnownNotifyObject knownObject) &&
         knownObject.Properties.TryGetValue(args.PropertyName, out KnownNotifyProperty knownProperty) &&
         value != knownProperty.Value) {
        knownObject.Properties.Remove(args.PropertyName);
        Detach(knownProperty.Value as INotifyPropertyChanged);
      }

      if (value is INotifyPropertyChanged notifyPropertyChanged) {
        Attach(sender, args.PropertyName, notifyPropertyChanged);
      } else if (value is INotifyCollectionChanged notifyCollectionChanged) {
        Attach(sender, args.PropertyName, notifyCollectionChanged);
      }

      PropertyChanged?.Invoke(sender, args);
    }
    #endregion

    #region INotifyCollectionChanged        
    public void Attach(INotifyCollectionChanged collection) => Attach(null, null, collection);

    public void Detach(INotifyCollectionChanged notifyCollectionChanged) {
      if (!(notifyCollectionChanged is IEnumerable collection)) {
        return;
      }

      knownCollections.Remove(collection);
      notifyCollectionChanged.CollectionChanged += OnCollectionChanged;

      foreach (var item in collection) {
        Detach(item as INotifyCollectionChanged);
        Detach(item as INotifyPropertyChanged);
      }
    }

    void Attach(object parent, string propertyName, INotifyCollectionChanged notifyCollectionChanged) {
      if (!(notifyCollectionChanged is IEnumerable collection)) {
        return;
      }

      if (parent != null & propertyName != null &&
          knownObjects.TryGetValue(parent as INotifyPropertyChanged, out KnownNotifyObject knownParent)) {
        if (knownParent.Properties.ContainsKey(propertyName)) {
          return;
        }

        knownParent.Properties.Add(propertyName, new KnownNotifyProperty {
          PropertyName = propertyName,
          Value = collection
        });
      }

      // Try to find object in internal list
      if (!knownCollections.Contains(collection)) {
        knownCollections.Add(collection);

        // Do not forget to subscribe to collection changed event
        notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
      }

      foreach (var item in collection) {
        Attach(item as INotifyCollectionChanged);
        Attach(item as INotifyPropertyChanged);
      }
    }

    void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
      void AddNewItems() {
        foreach (var item in e.NewItems) {
          Attach(item as INotifyCollectionChanged);
          Attach(item as INotifyPropertyChanged);
        }
      }
      void RemoveOldItems() {
        foreach (var item in e.OldItems) {
          Detach(item as INotifyCollectionChanged);
          Detach(item as INotifyPropertyChanged);
        }
      }
      switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
          AddNewItems(); 
          PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("Add"));
          break;
        case NotifyCollectionChangedAction.Remove:
          RemoveOldItems(); 
          PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("Remove"));
          break;
        case NotifyCollectionChangedAction.Replace:
          AddNewItems(); 
          RemoveOldItems();
          PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("Replace"));
          break;
        default:
          PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs("Any other fucking change"));
          break;
      }
    }
    #endregion

    IEnumerable<Tuple<string, T>> GetPropertiesOf<T>(object objectToExamine) where T : class {
      return objectToExamine.GetType()
          .GetProperties(BindingFlags.Public | BindingFlags.Instance)          
          .Where(p => !p.GetIndexParameters().Any()) // no indexer properties
          .Where(p => p.GetValue(objectToExamine) != null)
          .Where(p => p.GetValue(objectToExamine) is T)
          .Select(p => new Tuple<string, T>(p.Name, p.GetValue(objectToExamine) as T))
          .Where(p => p.Item2 != null);
    }
  }

  public class KnownNotifyObject {
    public object Parent { get; set; }
    public object Owner { get; set; }
    public IDictionary<string, KnownNotifyProperty> Properties = new Dictionary<string, KnownNotifyProperty>();
  }

  public class KnownNotifyProperty {
    public string PropertyName { get; set; }
    public object Value { get; set; }
  }
}
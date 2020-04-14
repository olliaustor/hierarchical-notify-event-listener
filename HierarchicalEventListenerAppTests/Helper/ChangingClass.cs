namespace HierarchicalEventListenerAppTests.Helper {
  using System.Collections.ObjectModel;
  using System.ComponentModel;

  public class ChangingClass : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    public ChangingClass(int id) => Id = id;
    public int Id { get; set; }

    string _Name;
    public string Name {
      get => _Name;
      set {
        _Name = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
      }
    }

    object _Tag;
    public object Tag {
      get => _Tag;
      set { _Tag = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tag))); }
    }

    ChangingClass _SubObject;
    public ChangingClass SubObject {
      get => _SubObject;
      set {
        _SubObject = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubObject)));
      }
    }

    ObservableCollection<ChangingClass> _Elements;
    public ObservableCollection<ChangingClass> Elements {
      get => _Elements ?? (_Elements = new ObservableCollection<ChangingClass>());
      set {
        _Elements = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Elements)));
      }
    }
  }
}
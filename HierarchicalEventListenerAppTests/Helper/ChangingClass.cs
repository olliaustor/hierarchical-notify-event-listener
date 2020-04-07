namespace HierarchicalEventListenerAppTests.Helper
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class ChangingClass : INotifyPropertyChanged
    {
        string name;
        ChangingClass subObject;
        ObservableCollection<ChangingClass> _Elements;

        public ChangingClass(int id) => Id = id;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public ChangingClass SubObject
        {
            get => subObject;
            set
            {
                subObject = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubObject)));
            }
        }
        public ObservableCollection<ChangingClass> Elements
        {
            get => _Elements ?? (_Elements = new ObservableCollection<ChangingClass>());
            set
            {
                _Elements = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Elements)));
            }
        }
    }
}

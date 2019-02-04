using System.Collections.ObjectModel;
using System.Windows.Input;
using Captura.Base;

namespace Captura.ViewCore.ViewModels
{
    public abstract class OverlayListViewModel<T> : NotifyPropertyChanged where T : class, new()
    {
        protected OverlayListViewModel(ObservableCollection<T> collection)
        {
            _collection = collection;

            Collection = new ReadOnlyObservableCollection<T>(_collection);

            AddCommand = new DelegateCommand(OnAddExecute);

            RemoveCommand = new DelegateCommand(OnRemoveExecute);

            if (collection.Count > 0)
            {
                SelectedItem = collection[0];
            }
        }

        private void OnAddExecute()
        {
            var item = new T();

            _collection.Add(item);

            SelectedItem = item;
        }

        private void OnRemoveExecute(object o)
        {
            if (o is T setting)
            {
                _collection.Remove(setting);
            }

            SelectedItem = _collection.Count > 0 ? _collection[0] : null;
        }

        private readonly ObservableCollection<T> _collection;

        public ReadOnlyObservableCollection<T> Collection { get; }

        public ICommand AddCommand { get; }

        public ICommand RemoveCommand { get; }

        private T _selectedItem;

        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                
                OnPropertyChanged();
            }
        }
    }
}
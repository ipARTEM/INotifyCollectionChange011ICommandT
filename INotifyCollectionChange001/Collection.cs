using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace INotifyCollectionChange011ICommandT
{
    public class Collection<T> :IList<T>,INotifyCollectionChanged
    {
        public Collection()  { }
        public Collection(Collection<T> source)  { list = source.ToList(); }
        List<T> list = new List<T>();

        public T this[int index] { get => list[index]; set => list[index] = value; }
        public int Count => list.Count;
        public bool IsReadOnly => false;
        public void Add(T item) => list.Add(item);
        public void Clear() => list.Clear();
        public bool Contains(T item) => list.Contains(item);
        public void CopyTo(T[] array, int index) => list.CopyTo(array,index);
        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        public int IndexOf(T item) => list.IndexOf(item);
        public void Insert(int index, T item) => list.Insert(index, item);
        public bool Remove(T item) => list.Remove(item);
        public void RemoveAt(int index) => list.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);
    }

    interface View<T> : ICollectionView
    {
        bool Contains(T item);
        bool MoveCurrentTo(T item);
        new Predicate<T> Filter { get; set; }
    }

    public class CollectionView<T> : View<T>
    {
        Collection<T> internalSource, realSourse;
        ICollectionView view;
        public CollectionView(Collection<T> source)
        {
            internalSource = new Collection<T>();
            realSourse = source;
            view = CollectionViewSource.GetDefaultView(internalSource);
            realSourse.CollectionChanged += OnSourceChanged;
        }

        private void OnSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int index = 0;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    index = internalSource.Count;
                    foreach (var item in e.NewItems)
                    {
                        internalSource.Add((T)item);
                    }
                    internalSource.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, e.NewItems, e.NewStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        index = internalSource.IndexOf((T)item);
                        
                        internalSource.RemoveAt(index);
                        internalSource.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item,index));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    index = internalSource.IndexOf((T)e.OldItems[0]);
                    internalSource[index] = (T)e.NewItems[0];
                    internalSource.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,e.NewItems[0], e.OldItems[0], index));
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    internalSource.Clear();
                    foreach(var item in realSourse)
                    {
                        internalSource.Add(item);
                    }
                    view.Refresh();
                    break;
                default:
                    break;
            }
        }

        public void Reposition(T item)
        {
            var index = internalSource.IndexOf(item);
            internalSource.RemoveAt(index);
            internalSource.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            internalSource.Add(item);
            internalSource.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void ApplyFilter(T item)
        {
            
            if (!Filter.Invoke(item))
            {
                var index = internalSource.IndexOf(item);
                internalSource.RemoveAt(index);
                internalSource.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
            else
            {

                internalSource.Add(item);
                internalSource.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));

            }
        }

        public bool CanFilter => view.CanFilter;
        public bool CanGroup => view.CanGroup;
        public bool CanSort => view.CanSort;
        public CultureInfo Culture { get => view.Culture; set =>view.Culture=value; }
        public object CurrentItem =>view.CurrentItem;
        public int CurrentPosition =>view.CurrentPosition;
        
        public ObservableCollection<GroupDescription> GroupDescriptions =>view.GroupDescriptions;
        public ReadOnlyObservableCollection<object> Groups =>view.Groups;
        public bool IsCurrentAfterLast =>view.IsCurrentAfterLast;
        public bool IsCurrentBeforeFirst => view.IsCurrentBeforeFirst;
        public bool IsEmpty =>view.IsEmpty;
        public SortDescriptionCollection SortDescriptions =>view.SortDescriptions;
        public IEnumerable SourceCollection =>internalSource;

        public event EventHandler CurrentChanged
        {
            add { view.CurrentChanged += value; }
            remove { view.CurrentChanged -= value; }
        }
        public event CurrentChangingEventHandler CurrentChanging
        {
            add { view.CurrentChanging += value; }
            remove { view.CurrentChanging -= value; }
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { view.CollectionChanged += value; }
            remove { view.CollectionChanged -= value; }
        }

        public IDisposable DeferRefresh() => view.DeferRefresh();
        public IEnumerator GetEnumerator() => internalSource.GetEnumerator();
        public bool MoveCurrentToFirst() => view.MoveCurrentToFirst();
        public bool MoveCurrentToLast() => view.MoveCurrentToLast();
        public bool MoveCurrentToNext() => view.MoveCurrentToNext();
        public bool MoveCurrentToPosition(int position) => view.MoveCurrentToPosition(position);
        public bool MoveCurrentToPrevious() => view.MoveCurrentToPrevious();
        public void Refresh() => view.Refresh();

        Predicate<T> filter;
        public Predicate<T>Filter 
        {
            get => filter;
            set { filter = value; view.Filter = new Predicate<object>((o) => value((T)o)); }
        }
        Predicate<object> ICollectionView.Filter { get => view.Filter; set => view.Filter = value; }
        public bool Contains(T item) => view.Contains(item);
        bool ICollectionView.Contains(object item) => view.Contains(item);
        public bool MoveCurrentTo(T item) => view.MoveCurrentTo(item);
        bool ICollectionView.MoveCurrentTo(object item) => view.MoveCurrentTo(item);
    }
}

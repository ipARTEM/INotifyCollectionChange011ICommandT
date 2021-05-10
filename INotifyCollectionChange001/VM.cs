using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace INotifyCollectionChange011ICommandT
{
    public class VM : Notifiable    
    {

        Collection<Item> items;
        public CollectionView<Item> Items { get; set; }
        public ICollectionView Items1 { get; set; } 
        string[] groups = { "Group 1", "Group 2", "Group 3" };

        public ICommand Add { get; set; }
        public ICommand Remove { get; set; }
        public ICommand Edit { get; set; }
        public ICommand Replace { get; set; }
        public ICommand Move { get; set; }
        public ICommand Clear { get; set; }
        public ICommand Hide { get; set; }
        public ICommand Show { get; set; }
        int count;
        Random random;

        public VM()
        {
            random = new Random();
            items = new Collection<Item>();
            Items = new CollectionView<Item>(items);

            Items.Filter = filter;
            Items.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Item.Group)));
            Items.SortDescriptions.Add(new SortDescription(nameof(Item.Name), ListSortDirection.Descending));

            Items1 = CollectionViewSource.GetDefaultView(items);

            Add = new Command<Item>(add, (o) => true);
            Remove = new Command<IEnumerable<object>>(remove, (o) => true);
            Edit = new Command<Item>(edit, (o) => true);
            Replace = new Command<IEnumerable<object>>(replace, (o) => true);
            Move = new Command<Item>(move, (o) => true);
            Clear = new Command<Item>((o)=>clear(), (o) => true);
            Hide = new Command<Item>(hide, (o) => true);
            Show = new Command<Item>(show, (o) => true);
        }

        void add(Item o)
        {
            var index = items.Count;
            var newItems = new List<Item>();

            for (int i = 0; i < 3; i++)
            {
                var item = new Item()
                {
                    Id = ++count,
                    Name = "Item No." + count,
                    Group = groups[random.Next(0, groups.Length)]
                };
                items.Add(item);
                newItems.Add(item);
            }
            
            items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,newItems,index));
            Items.MoveCurrentTo(items.Last());
            //List<Item> newItems = null;
            //Task.Run(() =>
            //{
            //    index = items.Count;
            //    for (int i = 0; i < 3; i++)
            //    {
            //        items.Add ( new Item()
            //        {
            //            Id = ++count,
            //            Name = "Item No." + count
            //        });
            //    }
            //    newItems = items.GetRange(index, items.Count - index);
               
            //}).Wait();
            //items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.GetRange(index, items.Count-index)));

                                             //Создание по 3 элемента
        //    for (int i = 0; i < 3; i++)
        //    {
        //        newItems.Add(new Item()
        //        {
        //            Id = ++count,
        //            Name = "Item No." + count
        //        });
        //    }
        //    if (items.Count == 0) items.AddRange(newItems);
        //    else items.InsertRange(items.Count, newItems);
        //}).Wait();
        //items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems));
        }

        void remove(IEnumerable<object> o)
        {
            var selectedItems = new List<Item>(o.Cast<Item>());
            foreach (var item in selectedItems)
            {
                var old = item as Item;
                var index = items.IndexOf(item);
                items.RemoveAt(index);
                items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,item,index));
            }

            
        }

        void edit(Item o)
        {

            
            o.Name = "Edited" + o.Id;
            o.Group = groups[random.Next(0, groups.Length)];
            o.OnPropertyChanged(nameof(o.Name));
            Items.Reposition(o);

        }

        void replace(IEnumerable<object> o)
        {
            var selectedItems = new List<Item>(o.Cast<Item>());
            foreach (var item in selectedItems)
            {
                
                var index = items.IndexOf(item);
                var @new = new Item()
                {
                    Id = item.Id,
                    Name = "Replaced" + item.Id,
                    Group=groups[random.Next(0,groups.Length)]
                };
                items[index] = @new;
                items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, @new, item, index));
                //App.Current.Dispatcher.Invoke(() => Items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, @new, old,index)));
            }
           
            //Items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            

            //2-й способ
            //var selectedItems = o as IEnumerable<object>;
            //var newItems = new List<Item>();

            //Task.Run(() =>
            //{
            //    foreach (var item in selectedItems)
            //    {
            //        var old = item as Item;
            //        var @new = new Item()
            //        {
            //            Id = old.Id,
            //            Name = "Replaced" + old.Id
            //        };
            //        items[items.IndexOf(old)] = @new;
            //        newItems.Add(@new);
            //    }

            //}).Wait();
            //items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, selectedItems));
        }

        void move(Item o)
        {
            
            int oldIndex, newIndex;
            oldIndex= newIndex = 0;


            Task.Run(() =>
            {
                
                oldIndex = items.IndexOf(o);
                newIndex = 0;
                items.Insert(newIndex, o);
            }).Wait();
            items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,o, newIndex, oldIndex));
        }

        void clear()
        {
            Task.Run(items.Clear).Wait();
            items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        void hide(Item o)
        {
            
            o.IsInvisible = true;
            o.OnPropertyChanged(nameof(o.IsInvisible));
            Items.ApplyFilter(o);
            //items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, index, index));
            //items.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, item, index));
        }
        void show(Item o)
        {
            var item = o as Item;
            o.IsInvisible = false;
            o.OnPropertyChanged(nameof(o.IsInvisible));
            Items.ApplyFilter(o);
        }

        bool filter(Item o) => !o.IsInvisible;

    }

    public class Item:Notifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsInvisible { get; set; }
        public string Group { get; set; }
    }
}

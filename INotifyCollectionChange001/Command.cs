using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace INotifyCollectionChange011ICommandT
{
    public class Command<T> : ICommand
    {
        Action<T> execute;
        Func<object, bool> canExecute;

        public Command(Action<T> execute, Func<object, bool> canExecute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => canExecute(parameter);

        public void Execute(object parameter)
        {
            //if(parameter is IEnumerable)
            //{
            //    var para = (IEnumerable<object>)parameter;
            //    var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(para.First().GetType()));
            //    var type =list.GetType();
            //    foreach (var item in para)
            //    {
            //        type.GetMethod("Add").Invoke(list, new[] { item });
            //    }
            //    execute((T)list);
            //}

            //else 
                execute((T)parameter);
        }

    }
}

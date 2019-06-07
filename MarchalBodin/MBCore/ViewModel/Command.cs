using System;
using System.Windows.Input;

namespace MBCore.ViewModel
{
    public class Command : ICommand
    {
        readonly Action<object> actionAExecuter;

        public Command(Action<object> execute)
            : this(execute, null)
        {
        }

        public Command(Action<object> execute, Predicate<object> canExecute)
        {
            actionAExecuter = execute;
        }

        public bool CanExecute(object parameter) => true;

#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
        public void Execute(object parameter)
        {
            actionAExecuter(parameter);
        }
    }
}

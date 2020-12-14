using System;
using System.Windows.Input;

namespace DirectoryInfo.Models
{
    public interface IRelayCommand : ICommand
    {
        void UpdateCanExecuteState();
    }

    public class RelayCommand : IRelayCommand
    {
        public event EventHandler CanExecuteChanged;

        readonly Predicate<Object> canExecute = null;
        readonly Action<Object> execute = null;

        public RelayCommand(Action execute, Predicate<Object> canExecute = null)
        {
            this.execute = (ex) => execute();
            this.canExecute = canExecute;
        }
        public RelayCommand(Action<object> executeAction, Predicate<Object> canExecute = null)
        {
            this.canExecute = canExecute;
            execute = executeAction;
        }


        public bool CanExecute(object parameter)
        {
            if (canExecute != null)
                return canExecute(parameter);
            return true;
        }

        public void UpdateCanExecuteState()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }

        public void Execute(object parameter)
        {
            if (execute != null)
                execute(parameter);
            UpdateCanExecuteState();
        }
    }
}

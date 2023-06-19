using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ScreenShare.ViewModel
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Predicate<object>? _canExecute;
        public event EventHandler? CanExecuteChanged;

        public DelegateCommand(Action execute, Predicate<object>? canExecute) 
        { 
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if(_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter!);
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}

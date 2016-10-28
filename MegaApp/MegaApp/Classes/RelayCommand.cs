using System;
using System.Windows.Input;

namespace MegaApp.Classes
{
    /// <summary>
    /// Typed ICommand interface implementation class
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this._execute((T)parameter);
            }
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// ICommand interface implementation class
    /// </summary>
    public class RelayCommand: ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;
        
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this._canExecute == null || this._canExecute();
        }

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                this._execute();
            }
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MegaApp.Classes
{
    /// <summary>
    /// Base ICommand interface implementation class
    /// </summary>
    public abstract class BaseRelayCommand : ICommand
    {
        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// ICommand interface implementation class
    /// </summary>
    public class RelayCommand : BaseRelayCommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) =>
            this._canExecute == null || this._canExecute();

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
                this._execute();
        }
    }

    /// <summary>
    /// Typed ICommand interface implementation class
    /// </summary>
    /// <typeparam name="T">Type of the object passed as parameter</typeparam>
    public class RelayCommand<T> : BaseRelayCommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) =>
            this._canExecute == null || this._canExecute((T)parameter);

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
                this._execute((T)parameter);
        }
    }

    /// <summary>
    /// Typed and asynchronous ICommand interface implementation class.
    /// </summary>
    /// <typeparam name="T">Type of the returned value</typeparam>
    public class RelayCommandAsync<T> : BaseRelayCommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Func<Task<T>> _execute;

        public RelayCommandAsync(Func<Task<T>> execute, Func<bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) =>
            this._canExecute == null || this._canExecute();

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
                this._execute();
        }

        public Task<T> ExecuteAsync(object parameter)
        {
            if (this.CanExecute(parameter))
                return this._execute();

            return Task.FromResult(default(T));
        }
    }

    /// <summary>
    /// Typed and asynchronous ICommand interface implementation class.
    /// </summary>
    /// <typeparam name="T1">Type of the object passed as parameter</typeparam>
    /// <typeparam name="T2">Type of the returned value</typeparam>
    public class RelayCommandAsync<T1, T2> : BaseRelayCommand
    {
        private readonly Func<T1,bool> _canExecute;
        private readonly Func<T1,Task<T2>> _execute;

        public RelayCommandAsync(Func<T1,Task<T2>> execute, Func<T1,bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) =>
            this._canExecute == null || this._canExecute((T1)parameter);

        public override void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
                this._execute((T1)parameter);
        }

        public Task<T2> ExecuteAsync(object parameter)
        {
            if (this.CanExecute(parameter))
                return this._execute((T1)parameter);

            return Task.FromResult(default(T2));
        }
    }
}

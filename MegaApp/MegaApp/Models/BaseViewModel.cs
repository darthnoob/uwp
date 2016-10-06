using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
//using System.Windows.Threading;
//using Microsoft.Phone.Reactive;

namespace MegaApp.Models
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected BaseViewModel()
        {
            this.ControlState = true;
            this.IsBusy = false;
        }

        #region Protected Methods

        /// <summary>
        /// Invoke the code/action on the UI Thread. If not on UI thread, dispatch to UI with the Dispatcher
        /// </summary>
        /// <param name="action">Action to invoke on the user interface thread</param>
        protected static async void OnUiThread(Action action)
        {
            // If no action then do nothing and return
            if(action == null) return;

            if(CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                // We are already on UI thread. Just invoke the action
                action.Invoke();
            }
            else
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, () => action.Invoke());
            }
        }

        #endregion

        #region Properties

        private bool _controlState;
        public bool ControlState
        {
            get { return _controlState; }
            set { SetField(ref _controlState, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetField(ref _isBusy, value); }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}

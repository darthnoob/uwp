using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MegaApp.ViewModels
{
    /// <summary>
    /// Base class of all the viewmodels. Implements INotifyPropertyChanged to inform
    /// the UI of changes to properties. 
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        protected BaseViewModel()
        {
            this.ControlState = true;
            this.IsBusy = false;
        }
        
        #region Properties

        /// <summary>
        /// State of the controls attached to this viewmodel
        /// </summary>
        private bool _controlState;
        public bool ControlState
        {
            get { return _controlState; }
            set { SetField(ref _controlState, value); }
        }

        /// <summary>
        /// is the viewmodel busy processing data
        /// </summary>
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

        protected bool SetField<T>(T currentValue, T newValue, Action doSet, [CallerMemberName] string property = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;
            doSet.Invoke();
            OnPropertyChanged(property);
            return true;
        }

        #endregion
    }
}

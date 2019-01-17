using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using mega;
using MegaApp.Services;

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

        /// <summary>
        /// State of the network connection
        /// </summary>
        public bool IsNetworkAvailable => NetworkService.HasInternetAccess();

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;

            try { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR,
                    string.Format("Error notifying that the property \"{0}\" has changed ", name), e);
            }
        }

        protected void OnPropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null || propertyNames.Length == 0) return;

            foreach (string name in propertyNames)
                OnPropertyChanged(name);
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

        #region UiResources

        public string OfflineBannerText => ResourceService.UiResources.GetString("UI_OfflineBanner");

        #endregion
    }
}

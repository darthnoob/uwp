using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MegaApp.Classes;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public abstract class SettingViewModel<T> : BaseUiViewModel, ISetting<T>
    {
        public event EventHandler ValueChanged;

        protected SettingViewModel(string title, string description, string key, T defaultValue = default(T))
        {
            this.Title = title;
            this.Description = description;
            this.Key = key;
            this.DefaultValue = defaultValue;
            this.ActionCommand = new RelayCommand(DoAction);
        }

        protected virtual void DoAction()
        {
            StoreValue(this.Key, this.Value);
        }

        public void Initialize()
        {
            this._value = this.GetValue(this.DefaultValue);
        }

        public virtual Task<bool> StoreValue(string key, T value)
        {
            return Task.FromResult(SettingsService.Save(key, value));
        }

        public T GetValue()
        {
            return GetValue(default(T));
        }

        public virtual T GetValue(T defaultValue)
        {
            return SettingsService.Load(this.Key, defaultValue);
        }

        #region Commands

        public ICommand ActionCommand { get; }

        #endregion

        #region Properties

        public string Title { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                if (SetField(ref _value, value))
                {
                    OnValueChanged();
                }
            }
        }

        public T DefaultValue { get; set; }

        object ISetting.Value
        {
            get { return this.Value; }
            set { this.Value = (T) value; }
        }
        
        #endregion

        #region Events

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}

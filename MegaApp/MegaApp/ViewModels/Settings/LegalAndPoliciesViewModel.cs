using System;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using MegaApp.Classes;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class LegalAndPoliciesSettingViewModel : SettingViewModel<object>
    {
        public LegalAndPoliciesSettingViewModel()
            : base(null, null, null)
        {
            this.CopyrightCommand = new RelayCommand(NavigateToCopyright);
            this.DataProtectionRegulationCommand = new RelayCommand(NavigateToDataProtectionRegulation);
            this.GeneralCommand = new RelayCommand(NavigateToGeneral);
            this.PrivacyPolicyCommand = new RelayCommand(NavigateToPrivacyPolicy);
            this.TakedownGuidanceCommand = new RelayCommand(NavigateToTakedownGuidance);
            this.TermsOfServiceCommand = new RelayCommand(NavigateToTermsOfService);
        }

        #region Commands

        public ICommand CopyrightCommand { get; private set; }
        public ICommand DataProtectionRegulationCommand { get; private set; }
        public ICommand GeneralCommand { get; private set; }
        public ICommand PrivacyPolicyCommand { get; private set; }
        public ICommand TakedownGuidanceCommand { get; private set; }
        public ICommand TermsOfServiceCommand { get; private set; }

        #endregion

        #region Methods

        public override object GetValue(object defaultValue)
        {
            return null;
        }

        public void UpdateWidthOfGUI(double actualWidth)
        {
            this.ViewAreaWidth = actualWidth >= this.viewAreaMaxWidth ?
                this.viewAreaMaxWidth : actualWidth;
        }

        private async void NavigateToCopyright()
        {
            await Launcher.LaunchUriAsync(new Uri(
                ResourceService.AppResources.GetString("AR_CopyrightUri"),
                UriKind.RelativeOrAbsolute));
        }

        private async void NavigateToDataProtectionRegulation()
        {
            await Launcher.LaunchUriAsync(new Uri(
                ResourceService.AppResources.GetString("AR_DataProtectionRegulationUri"),
                UriKind.RelativeOrAbsolute));
        }

        private async void NavigateToGeneral()
        {
            await Launcher.LaunchUriAsync(new Uri(
                ResourceService.AppResources.GetString("AR_GeneralLegalUri"),
                UriKind.RelativeOrAbsolute));
        }

        private async void NavigateToPrivacyPolicy()
        {
            await Launcher.LaunchUriAsync(new Uri(
                ResourceService.AppResources.GetString("AR_PrivacyPolicyUri"),
                UriKind.RelativeOrAbsolute));
        }

        private async void NavigateToTakedownGuidance()
        {
            await Launcher.LaunchUriAsync(new Uri(
                ResourceService.AppResources.GetString("AR_TakedownGuidanceUri"),
                UriKind.RelativeOrAbsolute));
        }

        private async void NavigateToTermsOfService()
        {
            await Launcher.LaunchUriAsync(new Uri(
                ResourceService.AppResources.GetString("AR_TermsOfServiceUri"),
                UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region Properties

        private double viewAreaMaxWidth => 
            (double)Application.Current.Resources["ViewAreaMaxWidth"];

        private double _viewAreaWidth;
        public double ViewAreaWidth
        {
            get { return _viewAreaWidth; }
            set
            {
                SetField(ref _viewAreaWidth, value);
                OnPropertyChanged(nameof(this.ColumnWidth));
            }
        }

        public GridLength ColumnWidth => new GridLength(ViewAreaWidth/2);

        #endregion

        #region UiResources

        public string CopyrightText => ResourceService.UiResources.GetString("UI_Copyright");
        public string DataProtectionRegulationText => ResourceService.UiResources.GetString("UI_DataProtectionRegulation");
        public string LearnMoreText => ResourceService.UiResources.GetString("UI_LearnMore");
        public string GeneralText => ResourceService.UiResources.GetString("UI_General");
        public string PrivacyPolicyText => ResourceService.UiResources.GetString("UI_PrivacyPolicy");
        public string TakedownGuidanceText => ResourceService.UiResources.GetString("UI_TakedownGuidance");
        public string TermsOfServiceText => ResourceService.UiResources.GetString("UI_TermsOfService");

        #endregion
    }
}

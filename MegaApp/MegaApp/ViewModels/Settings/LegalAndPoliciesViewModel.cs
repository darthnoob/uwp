using System.Collections.Generic;
using Windows.UI.Xaml;
using MegaApp.Services;

namespace MegaApp.ViewModels.Settings
{
    public class LegalAndPoliciesSettingViewModel : SettingViewModel<object>
    {
        public LegalAndPoliciesSettingViewModel()
            : base(null, null, null)
        {
            this.Items = new List<LinkSettingViewModel>();

            this.TermsOfServiceSetting = new LinkSettingViewModel(
                ResourceService.UiResources.GetString("UI_TermsOfService"),
                ResourceService.UiResources.GetString("UI_LearnMore"),
                ResourceService.AppResources.GetString("AR_TermsOfServiceUri"));
            this.Items.Add(this.TermsOfServiceSetting);

            this.PrivacyPolicySetting = new LinkSettingViewModel(
                ResourceService.UiResources.GetString("UI_PrivacyPolicy"),
                ResourceService.UiResources.GetString("UI_LearnMore"),
                ResourceService.AppResources.GetString("AR_PrivacyPolicyUri"));
            this.Items.Add(this.PrivacyPolicySetting);

            this.PrivacyPolicySetting = new LinkSettingViewModel(
                ResourceService.UiResources.GetString("UI_Copyright"),
                ResourceService.UiResources.GetString("UI_LearnMore"),
                ResourceService.AppResources.GetString("AR_CopyrightUri"));
            this.Items.Add(this.PrivacyPolicySetting);

            this.PrivacyPolicySetting = new LinkSettingViewModel(
                ResourceService.UiResources.GetString("UI_TakedownGuidance"),
                ResourceService.UiResources.GetString("UI_LearnMore"),
                ResourceService.AppResources.GetString("AR_TakedownGuidanceUri"));
            this.Items.Add(this.PrivacyPolicySetting);

            this.PrivacyPolicySetting = new LinkSettingViewModel(
                ResourceService.UiResources.GetString("UI_General"),
                ResourceService.UiResources.GetString("UI_LearnMore"),
                ResourceService.AppResources.GetString("AR_GeneralLegalUri"));
            this.Items.Add(this.PrivacyPolicySetting);

            this.PrivacyPolicySetting = new LinkSettingViewModel(
                ResourceService.UiResources.GetString("UI_DataProtectionRegulation"),
                ResourceService.UiResources.GetString("UI_LearnMore"),
                ResourceService.AppResources.GetString("AR_DataProtectionRegulationUri"));
            this.Items.Add(this.PrivacyPolicySetting);
        }

        #region Methods

        public override void Initialize()
        {
            foreach (var setting in this.Items)
                setting.Initialize();
        }

        public override object GetValue(object defaultValue)
        {
            return null;
        }
        
        #endregion

        #region Properties

        public IList<LinkSettingViewModel> Items { get; }
        
        public LinkSettingViewModel CopyrightSetting { get; }
        public LinkSettingViewModel DataProtectionRegulationSetting { get; }
        public LinkSettingViewModel GeneralLegalSetting { get; }
        public LinkSettingViewModel PrivacyPolicySetting { get; }
        public LinkSettingViewModel TakedownGuidanceSetting { get; }
        public LinkSettingViewModel TermsOfServiceSetting { get; }

        #endregion
    }
}

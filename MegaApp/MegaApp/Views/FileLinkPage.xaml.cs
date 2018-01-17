using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseFileLinkPage : PageEx<FileLinkViewModel> { }

    public sealed partial class FileLinkPage : BaseFileLinkPage
    {
        private const double ImportPanelMinWidth = 432;

        public FileLinkPage()
        {
            this.InitializeComponent();

            this.FileLinkSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsSplitViewOpenPropertyChanged);
        }

        private void IsSplitViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.ViewModel.IsPanelOpen)
            {
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.FileLinkSplitView.ActualWidth < 600)
                {
                    this.FileLinkSplitView.OpenPaneLength = this.FileLinkSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                switch (this.ViewModel.VisiblePanel)
                {
                    case PanelType.CopyMoveImport:
                        this.FileLinkSplitView.OpenPaneLength = ImportPanelMinWidth;
                        break;
                }
            }

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ImportPanelControl.ViewModel.ActionFinished -= OnImportFinished;
            this.ImportPanelControl.ViewModel.ActionCanceled -= OnImportCanceled;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ImportPanelControl.ViewModel.ActionFinished += OnImportFinished;
            this.ImportPanelControl.ViewModel.ActionCanceled += OnImportCanceled;

            this.ViewModel.GetPublicNode(LinkInformationService.ActiveLink);
        }

        private void OnImportFinished(object sender, EventArgs e)
        {
            this.ResetImport();

            // Navigate to the Cloud Drive page
            NavigateService.Instance.Navigate(typeof(CloudDrivePage), false,
                NavigationObject.Create(this.GetType()));
        }

        private void OnImportCanceled(object sender, EventArgs e)
        {
            this.ResetImport();
        }

        private void ResetImport()
        {
            this.ImportPanelControl.Reset();
            SelectedNodesService.ClearSelectedNodes();
            this.ViewModel.VisiblePanel = PanelType.None;
        }
    }
}

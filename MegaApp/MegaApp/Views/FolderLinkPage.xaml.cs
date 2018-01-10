using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;
using MegaApp.Classes;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseFolderLinkPage : PageEx<FolderLinkViewModel> { }

    public sealed partial class FolderLinkPage : BaseFolderLinkPage
    {
        private const double InformationPanelMinWidth = 432;
        private const double ImportPanelMinWidth = 432;

        public FolderLinkPage()
        {
            this.InitializeComponent();

            this.FolderLinkSplitView.RegisterPropertyChangedCallback(
                SplitView.IsPaneOpenProperty, IsSplitViewOpenPropertyChanged);
        }

        private void IsSplitViewOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (this.ViewModel.FolderLink.IsPanelOpen)
            {
                if (DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop || this.FolderLinkSplitView.ActualWidth < 600)
                {
                    this.FolderLinkSplitView.OpenPaneLength = this.FolderLinkSplitView.ActualWidth;
                    AppService.SetAppViewBackButtonVisibility(true);
                    return;
                }

                switch (this.ViewModel.FolderLink.VisiblePanel)
                {
                    case PanelType.Information:
                        this.FolderLinkSplitView.OpenPaneLength = InformationPanelMinWidth;
                        break;

                    case PanelType.CopyMoveImport:
                        this.FolderLinkSplitView.OpenPaneLength = ImportPanelMinWidth;
                        break;
                }
            }

            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        public override bool CanGoBack
        {
            get
            {
                bool canGoBack = false;
                if (this.ViewModel?.FolderLink != null)
                {
                    canGoBack = this.ViewModel.FolderLink.IsPanelOpen ||
                        this.ViewModel.FolderLink.CanGoFolderUp();
                }

                return canGoBack;
            }
        }

        public override void GoBack()
        {
            if (FolderLinkSplitView.IsPaneOpen)
                this.ViewModel.FolderLink.ClosePanels();
            else if (this.ViewModel.FolderLink.CanGoFolderUp())
                this.ViewModel.FolderLink.GoFolderUp();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.ViewModel.FolderLink.FolderNavigatedTo -= OnFolderNavigatedTo;
            this.ViewModel.FolderLink.SelectedNodesActionStarted -= OnImportStarted;
            this.ViewModel.FolderLink.SelectedNodesActionCanceled -= OnImportCanceled;

            this.ImportPanelControl.ViewModel.ActionFinished -= OnImportFinished;
            this.ImportPanelControl.ViewModel.ActionCanceled -= OnImportCanceled;

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel.FolderLink.FolderNavigatedTo += OnFolderNavigatedTo;
            this.ViewModel.FolderLink.SelectedNodesActionStarted += OnImportStarted;
            this.ViewModel.FolderLink.SelectedNodesActionCanceled += OnImportCanceled;

            this.ImportPanelControl.ViewModel.ActionFinished += OnImportFinished;
            this.ImportPanelControl.ViewModel.ActionCanceled += OnImportCanceled;

            this.ViewModel.LoginToFolder(LinkInformationService.ActiveLink);
        }

        private void OnFolderNavigatedTo(object sender, EventArgs eventArgs)
        {
            AppService.SetAppViewBackButtonVisibility(this.CanGoBack);
        }

        private void OnImportStarted(object sender, EventArgs e)
        {
            this.FolderLinkExplorer.DisableSelection();
        }

        private void OnImportFinished(object sender, EventArgs e)
        {
            ResetImport();

            // Navigate to the Cloud Drive page
            NavigateService.Instance.Navigate(typeof(CloudDrivePage), false,
                NavigationObject.Create(this.GetType(), NavigationActionType.Default));
        }

        private void OnImportCanceled(object sender, EventArgs e)
        {
            ResetImport();
        }

        private void ResetImport()
        {
            this.ViewModel.FolderLink.ResetSelectedNodes();
            this.ImportPanelControl.Reset();
            this.FolderLinkExplorer.ClearSelectedItems();
            this.FolderLinkExplorer.EnableSelection();
        }

        private void OnSortClick(object sender, RoutedEventArgs e)
        {
            var sortButton = sender as Button;
            if (sortButton == null) return;

            MenuFlyout menuFlyout = DialogService.CreateSortMenu(ViewModel.FolderLink);

            menuFlyout.Placement = FlyoutPlacementMode.Bottom;
            menuFlyout.ShowAt(sortButton);
        }
    }
}

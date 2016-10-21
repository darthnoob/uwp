using System;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;
using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseCloudDrivePage : PageEx<CloudDriveViewModel> { }

    public sealed partial class CloudDrivePage : BaseCloudDrivePage
    {
        public CloudDrivePage()
        {
            InitializeComponent();

            if(DeviceService.GetDeviceType() == DeviceFormFactorType.Phone)
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //_mainPageViewModel.Deinitialize(App.GlobalDriveListener);            
            base.OnNavigatedFrom(e);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavigationActionType navActionType = NavigateService.GetNavigationObject(e.Parameter).Action;

            // Need to check it always and no only in StartupMode, 
            // because this is the first page loaded
            if (!await AppService.CheckActiveAndOnlineSession(e.NavigationMode)) return;

            if (!await NetworkService.IsNetworkAvailable())
            {
                //UpdateGUI(false);
                return;
            }

            switch(navActionType)
            {
                case NavigationActionType.Login:
                    ViewModel.FetchNodes();
                    break;

                case NavigationActionType.Default:
                    break;
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            args.Handled = ProcessBackRequest(args.Handled);
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = ProcessBackRequest(e.Handled);
        }

        public bool ProcessBackRequest(bool isHandled)
        {
            // Check if we can go a folder up in the selected pivot view
            isHandled = CheckAndGoFolderUp(isHandled);

            // If no folder up action, but we are not in the cloud drive section
            // first slide to cloud drive before exiting the application
            isHandled = CheckPivotInView(isHandled);

            return isHandled;
        }

        private bool CheckAndGoFolderUp(bool isHandled)
        {
            try
            {
                if (isHandled) return true;

                if(MainPivot.SelectedItem != null && ViewModel != null)
                {
                    if ((MainPivot.SelectedItem as PivotItem).Equals(CloudDrivePivot) && ViewModel.CloudDrive != null)
                        return ViewModel.CloudDrive.GoFolderUp();

                    if ((MainPivot.SelectedItem as PivotItem).Equals(RubbishBinPivot) && ViewModel.RubbishBin != null)
                        return ViewModel.RubbishBin.GoFolderUp();
                }

                return false;
            }
            catch (Exception) { return false; }
        }

        private bool CheckPivotInView(bool isHandled)
        {
            try
            {
                if (isHandled) return true;

                if (MainPivot.SelectedItem != null && ViewModel != null)
                {
                    if ((MainPivot.SelectedItem as PivotItem).Equals(CloudDrivePivot) && ViewModel.CloudDrive != null)
                        return false;

                    MainPivot.SelectedItem = CloudDrivePivot;
                    return true;
                }

                return false;
            }
            catch (Exception) { return false; }
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((PivotItem)(sender as Pivot).SelectedItem).Equals(CloudDrivePivot))
                ViewModel.ActiveFolderView = ((CloudDriveViewModel)DataContext).CloudDrive;

            if (((PivotItem)(sender as Pivot).SelectedItem).Equals(RubbishBinPivot))
                ViewModel.ActiveFolderView = ((CloudDriveViewModel)DataContext).RubbishBin;                
        }

        private void OnItemTapped(object sender, TappedRoutedEventArgs e)
        {
            if(DeviceService.GetDeviceType() != DeviceFormFactorType.Desktop)
                ProcessItemTapped(((ListView)sender).SelectedItem);
        }

        private void OnItemDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ProcessItemTapped(((ListView)sender).SelectedItem);
        }

        private void ProcessItemTapped(object itemTapped)
        {
            if (itemTapped != null)
            {
                LstCloudDrive.SelectedItem = null;
                ViewModel.ActiveFolderView.OnChildNodeTapped((IMegaNode)itemTapped);
            }
        }

        private void OnListLoaded(object sender, RoutedEventArgs e)
        {

        }        
    }
}

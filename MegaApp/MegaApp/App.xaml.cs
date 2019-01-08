using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.MegaApi;
using MegaApp.Services;
using MegaApp.Views;
using MegaApp.ViewModels;
using MegaApp.ViewModels.Dialogs;

namespace MegaApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application, MRequestListenerInterface
    {
        /// <summary>
        /// Provides easy access to usefull application information
        /// </summary>
        public static AppInformation AppInformation { get; private set; }
        public static string IpAddress { get; set; }

        /// <summary>
        /// Global notifications listener
        /// </summary>
        public static GlobalListener GlobalListener { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Standard XAML initialization
            this.InitializeComponent();

            // App initialization
            InitializeApplication();

            this.Suspending += OnSuspending;
            this.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. 
        /// Other entry points will be used in specific cases, such as when the 
        /// application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
            // Load product xml for IAP testing
            await LicenseService.LoadSimulatorAsync();
#endif
            Frame rootFrame = CreateRootFrame();

            if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: Load state from previously suspended application
            }

            if (e.PrelaunchActivated == false)
            {
                // When the navigation stack isn't restored navigate to the first page, configuring 
                // the new page by passing required information as a navigation parameter
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Handle protocol activations.
        /// </summary>
        /// <param name="e">Details about the activate request and process.</param>
        protected override async void OnActivated(IActivatedEventArgs e)
        {
            if (e.Kind == ActivationKind.Protocol)
            {
                // Handle URI activation
                ProtocolActivatedEventArgs eventArgs = e as ProtocolActivatedEventArgs;

                // Initialize the links information
                LinkInformationService.Reset();

                bool validUri = true;
                Exception exception = null;
                try
                {
                    validUri = eventArgs.Uri.IsWellFormedOriginalString();
                    if (validUri)
                    {
                        // Use OriginalString to keep uppercase and lowercase letters
                        LinkInformationService.ActiveLink = UriService.ReformatUri(eventArgs.Uri.OriginalString);
                    }
                }
                catch (UriFormatException ex)
                {
                    validUri = false;
                    exception = ex;
                }
                finally
                {
                    if(!validUri)
                    {
                        LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Invalid URI detected during app activation", exception);
                        await DialogService.ShowAlertAsync(ResourceService.AppMessages.GetString("AM_InvalidUri_Title"),
                            ResourceService.AppMessages.GetString("AM_InvalidUri"));
                    }
                }

                Frame rootFrame = CreateRootFrame();

                if (eventArgs.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // When the navigation stack isn't restored navigate to the first page, configuring 
                // the new page by passing required information as a navigation parameter
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), eventArgs);

                // Ensure the current window is active
                Window.Current.Activate();

                // Check session and special navigation
                await AppService.CheckActiveAndOnlineSessionAsync();

                // Validate product subscription license on background thread
                Task.Run(() => LicenseService.ValidateLicensesAsync());
            }
        }

        /// <summary>
        /// Get the current root frame or create a new one if not exists
        /// </summary>
        /// <returns>The app root frame</returns>
        private Frame CreateRootFrame()
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // Add rootFrame as mainframe to navigation service
                NavigateService.CoreFrame = rootFrame;

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Variable which indicates if the app is being forcing to crash
        /// after manage an unhnadled exception
        /// </summary>
        private bool isAborting;

        /// <summary>
        /// Invoked when occurs an unhandled exception.
        /// </summary>
        /// <param name="sender">The source of the unhandled exception.</param>
        /// <param name="e">Details about the unhandled exception.</param>
        private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                // Save the exception in a local variable to preserve the stack trace
                var exception = e.Exception;

                // An unhandled exception has occurred. Break into the debugger
                if (Debugger.IsAttached)
                    Debugger.Break();

                if (isAborting) return;

                e.Handled = true;

                string message = string.Format("{0}{1}{1}{2}", ResourceService.AppMessages.GetString("AM_ApplicationErrorParagraph1"),
                        Environment.NewLine, ResourceService.AppMessages.GetString("AM_ApplicationErrorParagraph2"));

                var result = await DialogService.ShowOkCancelAsync(
                    ResourceService.AppMessages.GetString("AM_ApplicationError_Title"), message,
                    TwoButtonsDialogType.YesNo);

                if (result)
                    await DebugService.ComposeErrorReportEmailAsync(exception);
            }
            catch (Exception ex)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error managing an unhandled exception.", ex);
            }
            finally
            {
                // Reenabling auto crash
                ForceAppCrash(e.Exception);
            }
        }

        /// <summary>
        /// Force the app crash after manage an unhandled exception 
        /// to register the crash into the automatic exception tracker.
        /// </summary>
        /// <param name="unhandledEx">Unhandled exception</param>
        private void ForceAppCrash(Exception unhandledEx)
        {
            try
            {
                isAborting = true;
                ExceptionDispatchInfo.Capture(unhandledEx).Throw();
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error forcing app crash after manage an unhandled exception.", e);
            }
        }

        #region Application initialization

        // Avoid double-initialization
        private bool ApplicationInitialized = false;

        private void InitializeApplication()
        {
            if (ApplicationInitialized) return;

            // Clear settings values we do no longer use
            AppService.ClearObsoleteSettings();

            // Initialize the application information
            if (AppInformation == null)
                AppInformation = new AppInformation();

            // Initialize the links information
            LinkInformationService.Reset();

            // Initialize SDK parameters
            SdkService.InitializeSdkParams();

            // Add a global notifications listener
            GlobalListener = new GlobalListener();
            SdkService.MegaSdk.addGlobalListener(GlobalListener);

            // Add a global request listener to process all.
            SdkService.MegaSdk.addRequestListener(this);

            // Add a global transfer listener to process all transfers.            
            SdkService.MegaSdk.addTransferListener(TransferService.GlobalTransferListener);

            // Initialize Folders
            AppService.InitializeAppFolders();

            // Initialize the DB
            AppService.InitializeDatabase();

            // Save the app information for future use (like deleting settings)
            AppService.SaveAppInformation();

            // Ensure we don't initialize again
            ApplicationInitialized = true;
        }

        #endregion

        #region MRequestListenerInterface

        // Avoid show multiple SSL certificate alerts
        private bool SSLCertificateAlertDisplayed = false;

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            switch(e.getErrorCode())
            {
                case MErrorType.API_EINCOMPLETE:
                    if(request.getType() == MRequestType.TYPE_LOGOUT &&
                        request.getParamType() == (int)MErrorType.API_ESSL)
                    {
                        if (SSLCertificateAlertDisplayed) break;

                        SSLCertificateAlertDisplayed = true;
                        UiService.OnUiThread(async() =>
                        {
                            var result = await DialogService.ShowSSLCertificateAlert();
                            SSLCertificateAlertDisplayed = false;
                            switch (result)
                            {
                                // "Retry" button
                                case ContentDialogResult.Primary:
                                    api.reconnect();
                                    break;

                                // "Open browser" button
                                case ContentDialogResult.Secondary:
                                    await Launcher.LaunchUriAsync(
                                        new Uri(ResourceService.AppResources.GetString("AR_MegaUrl"),
                                        UriKind.RelativeOrAbsolute));
                                    break;

                                // "Ignore" or "Close" button
                                case ContentDialogResult.None:
                                default:
                                    api.setPublicKeyPinning(false);
                                    api.reconnect();
                                    break;
                            }
                        });
                    }
                    break;

                case MErrorType.API_ESID:
                    AppService.LogoutActions();

                    // Show the login page with the corresponding navigation parameter
                    UiService.OnUiThread(() =>
                    {
                        NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true,
                            NavigationObject.Create(typeof(MainViewModel), NavigationActionType.API_ESID));
                    });
                    break;
            }
        }

        public virtual void onRequestStart(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        public virtual void onRequestTemporaryError(MegaSDK api, MRequest request, MError e)
        {
            // Not necessary
        }

        public virtual void onRequestUpdate(MegaSDK api, MRequest request)
        {
            // Not necessary
        }

        #endregion
    }
}

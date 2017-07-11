using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
        /// Provides easy access to usefull links information
        /// </summary>
        public static LinkInformation LinkInformation { get; set; }

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
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. 
        /// Other entry points will be used in specific cases, such as when the 
        /// application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
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
                if (LinkInformation == null)
                    LinkInformation = new LinkInformation();

                bool validUri = true;
                Exception exception = null;
                try
                {
                    validUri = eventArgs.Uri.IsWellFormedOriginalString();
                    if (validUri)
                        LinkInformation.ActiveLink = UriService.ReformatUri(eventArgs.Uri.AbsoluteUri);
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
                if (await AppService.CheckActiveAndOnlineSession())
                    await AppService.CheckSpecialNavigation();
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

        #region Application initialization

        // Avoid double-initialization
        private bool ApplicationInitialized = false;

        private void InitializeApplication()
        {
            if (ApplicationInitialized) return;

            // Initialize the application information
            if(AppInformation == null)
                AppInformation = new AppInformation();

            // Initialize the links information
            if (LinkInformation == null)
                LinkInformation = new LinkInformation();

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

            // Ensure we don't initialize again
            ApplicationInitialized = true;
        }

        #endregion

        #region MRequestListenerInterface

        public virtual void onRequestFinish(MegaSDK api, MRequest request, MError e)
        {
            if (e.getErrorCode() == MErrorType.API_ESID || e.getErrorCode() == MErrorType.API_ESSL)
            {
                AppService.LogoutActions();

                // Show the login page with the corresponding navigation parameter
                if (e.getErrorCode() == MErrorType.API_ESID)
                {
                    UiService.OnUiThread(() =>
                    {
                        NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true,
                            NavigationObject.Create(typeof(MainViewModel), NavigationActionType.API_ESID));
                    });
                }
                else if (e.getErrorCode() == MErrorType.API_ESSL)
                {
                    UiService.OnUiThread(() =>
                    {
                        NavigateService.Instance.Navigate(typeof(LoginAndCreateAccountPage), true,
                            NavigationObject.Create(typeof(MainViewModel), NavigationActionType.API_ESSL));
                    });
                }
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

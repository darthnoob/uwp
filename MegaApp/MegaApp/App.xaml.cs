using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using mega;
using MegaApp.Classes;
using MegaApp.MegaApi;
using MegaApp.Pages;
using MegaApp.Resources;
using MegaApp.Services;

namespace MegaApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Provides easy access to usefull application information
        /// </summary>
        public static AppInformation AppInformation { get; private set; }
        public static String IpAddress { get; set; }

        /// <summary>
        /// Provides easy access to the resources strings
        /// </summary>
        public static ResourceLoaders ResourceLoaders { get; private set; }

        /// <summary>
        /// Main MegaSDK instance of the app
        /// </summary>
        public static MegaSDK MegaSdk { get; set; }

        /// <summary>
        /// MegaSDK instance for the folder links management
        /// </summary>
        public static MegaSDK MegaSdkFolderLinks { get; set; }

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
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
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
            AppInformation = new AppInformation();

            // Initialize the resource loaders
            ResourceLoaders = new ResourceLoaders();

            //The next line enables a custom logger, if this function is not used OutputDebugString() is called
            //in the native library and log messages are only readable with the native debugger attached.
            //The default behavior of MegaLogger() is to print logs using Debug.WriteLine() but it could
            //be used to sends log to a file, for example.
            MegaSDK.setLoggerObject(new MegaLogger());

            //You can select the maximum output level for debug messages.
            //By default FATAL, ERROR, WARNING and INFO will be enabled
            //DEBUG and MAX can only be enabled in Debug builds, they are ignored in Release builds
            MegaSDK.setLogLevel(MLogLevel.LOG_LEVEL_DEBUG);

            //You can send messages to the logger using MEGASDK.log(), those messages will be received
            //in the active logger
            MegaSDK.log(MLogLevel.LOG_LEVEL_INFO, "Example log message");

            // Set the ID for statistics
            MegaSDK.setStatsID(AppService.GetDeviceID());

            // Get an instance of the object that allow recover the local device information.
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();

            // Initialize the main MegaSDK instance
            MegaSdk = new MegaSDK("Z5dGhQhL", String.Format("{0}/{1}/{2}", 
                AppService.GetAppUserAgent(), deviceInfo.SystemManufacturer, deviceInfo.SystemProductName),
                ApplicationData.Current.LocalFolder.Path, new MegaRandomNumberProvider());

            // Initialize the MegaSDK instance for Folder Links
            MegaSdkFolderLinks = new MegaSDK("Z5dGhQhL", String.Format("{0}/{1}/{2}", 
                AppService.GetAppUserAgent(), deviceInfo.SystemManufacturer, deviceInfo.SystemProductName),
                ApplicationData.Current.LocalFolder.Path, new MegaRandomNumberProvider());

            // Ensure we don't initialize again
            ApplicationInitialized = true;
        }

        #endregion
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityPlayer;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace HolographicPhotoProject
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        public MainPage MainPageRef;
        public int MainViewId;
        public int BundlesViewId;
        public int TemplatesViewId;

        private AppCallbacks appCallbacks;
        public SplashScreen SplashScreen;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            SetupOrientation();
            appCallbacks = new AppCallbacks();
        }

        /// <summary>
        /// Invoked when application is launched through protocol.
        /// Read more - http://msdn.microsoft.com/library/windows/apps/br224742
        /// </summary>
        /// <param name="args"></param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            string appArgs = String.Empty;

            switch (args.Kind)
            {
                case ActivationKind.Protocol:
                    ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                    SplashScreen = eventArgs.SplashScreen;
                    appArgs += string.Format("Uri={0}", eventArgs.Uri.AbsoluteUri);
                    break;
            }
            InitializeUnity(appArgs);
        }

        /// <summary>
        /// Invoked when application is launched via file
        /// Read more - http://msdn.microsoft.com/library/windows/apps/br224742
        /// </summary>
        /// <param name="args"></param>
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            string appArgs = String.Empty;

            SplashScreen = args.SplashScreen;
            appArgs += "File=";
            bool firstFileAdded = false;
            foreach (var file in args.Files)
            {
                if (firstFileAdded)
                {
                    appArgs += ";";
                }
                appArgs += file.Path;
                firstFileAdded = true;
            }

            InitializeUnity(appArgs);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            SplashScreen = args.SplashScreen;
            InitializeUnity(args.Arguments);
        }

        private void InitializeUnity(string args)
        {
#if UNITY_WP_8_1 || UNITY_UWP
            ApplicationView.GetForCurrentView().SuppressSystemOverlays = true;
#if UNITY_UWP
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
#endif
#pragma warning disable 4014
            {
                StatusBar.GetForCurrentView().HideAsync();
            }
#pragma warning restore 4014
#endif
            appCallbacks.SetAppArguments(args);
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null && !appCallbacks.IsInitialized())
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
#if !UNITY_HOLOGRAPHIC
				Window.Current.Activate();
#endif
                rootFrame.Navigate(typeof(MainPage));
                MainPageRef = rootFrame.Content as MainPage;
                MainViewId = ApplicationView.GetForCurrentView().Id;
            }

            Window.Current.Activate();
        }

        private void SetupOrientation()
        {
#if UNITY_UWP
            Unity.UnityGenerated.SetupDisplay();
#endif
        }
    }
}

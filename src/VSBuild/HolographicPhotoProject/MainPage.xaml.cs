﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.ViewModels;
using HolographicPhotoProject.Utilities;
using System;
using UnityPlayer;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HolographicPhotoProject
{
    public sealed partial class MainPage : Page
    {
        private App app = Application.Current as App;
                
        private WinRTBridge.WinRTBridge bridge;

        private SplashScreen splash;
        private Rect splashImageRect;
        private WindowSizeChangedEventHandler onResizeHandler;
#if UNITY_WP_8_1
		private TypedEventHandler<DisplayInformation, object> onRotationChangedHandler;
#endif
        private bool isPhone = false;
        private MainPageViewModel mainPageViewModel;
        
        public MainPage()
        {
            this.InitializeComponent();
            mainPageViewModel = new MainPageViewModel();
            DataContext = mainPageViewModel;
            Authenticate();
            
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            
            AppCallbacks appCallbacks = AppCallbacks.Instance;
            // Setup scripting bridge
            bridge = new WinRTBridge.WinRTBridge();
            appCallbacks.SetBridge(bridge);
            
            // Sets the delegate method on the ViewSwitcher so we can call UWP code...
          FlatAppHelper.NavigateToFlatApp = SwitchToFlatPage;

            bool isWindowsHolographic = false;

#if UNITY_HOLOGRAPHIC
            // If application was exported as Holographic check if the deviceFamily actually supports it,
            // otherwise we treat this as a normal XAML application
            string deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
            isWindowsHolographic = String.Compare("Windows.Holographic", deviceFamily) == 0;
#endif

            if (isWindowsHolographic)
            {
                appCallbacks.InitializeViewManager(Window.Current.CoreWindow);
            }
            else
            {
                appCallbacks.RenderingStarted += () => { RemoveSplashScreen(); };

#if UNITY_UWP
                if (Windows.Foundation.Metadata.ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1))
                {
                    isPhone = true;
                }
#endif
#if !UNITY_WP_8_1
                appCallbacks.SetKeyboardTriggerControl(this);
#else
				isPhone = true;
#endif
                appCallbacks.SetSwapChainPanel(GetSwapChainPanel());
                appCallbacks.SetCoreWindowEvents(Window.Current.CoreWindow);
                appCallbacks.InitializeD3DXAML();

                splash = ((App)App.Current).SplashScreen;
                GetSplashBackgroundColor();
                OnResize();
                onResizeHandler = new WindowSizeChangedEventHandler((o, e) => OnResize());
                Window.Current.SizeChanged += onResizeHandler;

#if UNITY_WP_8_1
				onRotationChangedHandler = new TypedEventHandler<DisplayInformation, object>((di, o) => { OnRotate(di); });
				ExtendedSplashImage.RenderTransformOrigin = new Point(0.5, 0.5);
				var displayInfo = DisplayInformation.GetForCurrentView();
				displayInfo.OrientationChanged += onRotationChangedHandler;
				OnRotate(displayInfo);

				SetupLocationService();
#endif
            }
        }

        /// <summary>
        /// Attempt to authenticate to OneDrive.
        /// </summary>
        public async void Authenticate()
        {
            if (mainPageViewModel != null)
            {
                await mainPageViewModel.AuthenticateAsync();
            }
        }

        /// <summary>
        /// This method creates a new view for our flat XAML page. It also saves off the view ID so we can navigate to the new view later.
        /// Additionally, this sets a delegate in the Unity script AppViewSwitcher, which will allow it to call a method in this file from the Unity view.
        /// </summary>
        public async void SetUpBundlesView()
        {
            CoreApplicationView bundlesPageView = CoreApplication.CreateNewView();
            await bundlesPageView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                app.BundlesViewId = ApplicationView.GetForCurrentView().Id;
                Frame frame = new Frame();
                frame.Navigate(typeof(BundlesPage));
                Window.Current.Content = frame;
                Window.Current.Activate();
            });
        }

        /// <summary>
        /// This switches from the Unity exclusive view to our XAML flat panel, if it exists. If not, we try to set the new view up again.
        /// </summary>
        public async void SwitchToFlatPage()
        {
            if (app.BundlesViewId == 0)
            {
                SetUpBundlesView();
            }

            if (app.BundlesViewId != 0)
            {
                await ApplicationViewSwitcher.SwitchAsync(app.BundlesViewId, app.MainViewId, ApplicationViewSwitchingOptions.ConsolidateViews);
            }
        }
        
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            splash = (SplashScreen)e.Parameter;
            OnResize();
        }

        private void OnResize()
        {
            if (splash != null)
            {
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }
        }

#if UNITY_WP_8_1
		private void OnRotate(DisplayInformation di)
		{
			// system splash screen doesn't rotate, so keep extended one rotated in the same manner all the time
			int angle = 0;
			switch (di.CurrentOrientation)
			{
			case DisplayOrientations.Landscape:
				angle = -90;
				break;
			case DisplayOrientations.LandscapeFlipped:
				angle = 90;
				break;
			case DisplayOrientations.Portrait:
				angle = 0;
				break;
			case DisplayOrientations.PortraitFlipped:
				angle = 180;
				break;
			}
			var rotate = new RotateTransform();
			rotate.Angle = angle;
			ExtendedSplashImage.RenderTransform = rotate;
		}
#endif

        private void PositionImage()
        {
            var inverseScaleX = 1.0f;
            var inverseScaleY = 1.0f;
            if (isPhone)
            {
                inverseScaleX = inverseScaleX / DXSwapChainPanel.CompositionScaleX;
                inverseScaleY = inverseScaleY / DXSwapChainPanel.CompositionScaleY;
            }

            ExtendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X * inverseScaleX);
            ExtendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y * inverseScaleY);
            ExtendedSplashImage.Height = splashImageRect.Height * inverseScaleY;
            ExtendedSplashImage.Width = splashImageRect.Width * inverseScaleX;
        }

        private async void GetSplashBackgroundColor()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///AppxManifest.xml"));
                string manifest = await FileIO.ReadTextAsync(file);
                int idx = manifest.IndexOf("SplashScreen");
                manifest = manifest.Substring(idx);
                idx = manifest.IndexOf("BackgroundColor");
                if (idx < 0)  // background is optional
                {
                    return;
                }

                manifest = manifest.Substring(idx);
                idx = manifest.IndexOf("\"");
                manifest = manifest.Substring(idx + 1);
                idx = manifest.IndexOf("\"");
                manifest = manifest.Substring(0, idx);
                int value = 0;
                bool transparent = false;
                if (manifest.Equals("transparent"))
                {
                    transparent = true;
                }
                else if (manifest[0] == '#') // color value starts with #
                {
                    value = Convert.ToInt32(manifest.Substring(1), 16) & 0x00FFFFFF;
                }
                else
                {
                    return; // at this point the value is 'red', 'blue' or similar, Unity does not set such, so it's up to user to fix here as well
                }

                byte r = (byte)(value >> 16);
                byte g = (byte)((value & 0x0000FF00) >> 8);
                byte b = (byte)(value & 0x000000FF);

                await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.High, delegate()
                {
                    byte a = (byte)(transparent ? 0x00 : 0xFF);
                    ExtendedSplashGrid.Background = new SolidColorBrush(Color.FromArgb(a, r, g, b));
                });
            }
            catch (Exception)
            { }
        }

        public SwapChainPanel GetSwapChainPanel()
        {
            return DXSwapChainPanel;
        }

        public void RemoveSplashScreen()
        {
            DXSwapChainPanel.Children.Remove(ExtendedSplashGrid);
            if (onResizeHandler != null)
            {
                Window.Current.SizeChanged -= onResizeHandler;
                onResizeHandler = null;

#if UNITY_WP_8_1
				DisplayInformation.GetForCurrentView().OrientationChanged -= onRotationChangedHandler;
				onRotationChangedHandler = null;
#endif
            }
        }

#if !UNITY_WP_8_1
        protected override Windows.UI.Xaml.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new UnityPlayer.XamlPageAutomationPeer(this);
        }
#else
		// This is the default setup to show location consent message box to the user
		// You can customize it to your needs, but do not remove it completely if your application
		// uses location services, as it is a requirement in Windows Store certification process
		private async void SetupLocationService()
		{
			AppCallbacks appCallbacks = AppCallbacks.Instance;
			if (!appCallbacks.IsLocationCapabilitySet())
			{
				return;
			}

			const string settingName = "LocationContent";
			bool userGaveConsent = false;

			object consent;
			var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
			var userWasAskedBefore = settings.Values.TryGetValue(settingName, out consent);

			if (!userWasAskedBefore)
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("Can this application use your location?", "Location services");

				var acceptCommand = new Windows.UI.Popups.UICommand("Yes");
				var declineCommand = new Windows.UI.Popups.UICommand("No");

				messageDialog.Commands.Add(acceptCommand);
				messageDialog.Commands.Add(declineCommand);

				userGaveConsent = (await messageDialog.ShowAsync()) == acceptCommand;
				settings.Values.Add(settingName, userGaveConsent);
			}
			else
			{
				userGaveConsent = (bool)consent;
			}

			if (userGaveConsent)
			{	// Must be called from UI thread
				appCallbacks.SetupGeolocator();
			}
		}
#endif
    }
}

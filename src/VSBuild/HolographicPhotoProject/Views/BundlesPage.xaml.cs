// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Models;
using HolographicPhotoProject.ViewModels;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HolographicPhotoProject
{
    public sealed partial class BundlesPage : Page
    {
        private App app = Application.Current as App;
        private BundlesPageViewModel bundlesPageViewModel = null;

        public static class Constants
        {
            public const string IgnoreRefreshSource = "Button";
        }

        public BundlesPage()
        {
            this.InitializeComponent();
            if (bundlesPageViewModel == null)
            {
                bundlesPageViewModel = new BundlesPageViewModel();
                this.DataContext = bundlesPageViewModel;
            }
            bundlesPageViewModel.GoToMainPage = new RelayCommand<object>(async param =>
            {
                if (app.MainViewId != 0)
                {
                    bundlesPageViewModel.LoadExistingSouvenirs();
                    await ApplicationViewSwitcher.SwitchAsync(app.MainViewId, app.BundlesViewId, ApplicationViewSwitchingOptions.ConsolidateViews);
                }
            }, (object param) => true);
            bundlesPageViewModel.GoToTemplatePage = new RelayCommand<object>(async param =>
            {
                if (app.TemplatesViewId == 0)
                {
                    CoreApplicationView TemplatesPageView = CoreApplication.CreateNewView();
                    await TemplatesPageView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var templatesPageViewModel = new TemplatesPageViewModel();
                        app.TemplatesViewId = ApplicationView.GetForCurrentView().Id;
                        Frame frame = new Frame();
                        templatesPageViewModel.SelectedSouvenirModel = bundlesPageViewModel.SelectedSouvenirModel;
                        frame.Navigate(typeof(TemplatesPage), templatesPageViewModel);
                        Window.Current.Content = frame;
                        Window.Current.Activate();
                    });
                }

                if (app.TemplatesViewId != 0)
                {
                    bundlesPageViewModel.SelectedSouvenirModel.BundleId = (param as BundleModel)?.Id;
                    await ApplicationViewSwitcher.SwitchAsync(app.TemplatesViewId, app.BundlesViewId, ApplicationViewSwitchingOptions.ConsolidateViews);
                }
                else
                {
                    Debug.WriteLine("Error: TemplatesPageId is not set.");
                }
            }, (object param) => true);

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += BundlesPage_BackRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bundlesPageViewModel.Refresh();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            bundlesPageViewModel.DispatchTimerStart();
            if (!Constants.IgnoreRefreshSource.Equals(e.OriginalSource.GetType().Name, StringComparison.OrdinalIgnoreCase))
            {
                bundlesPageViewModel.Refresh();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
              bundlesPageViewModel.DispatchTimerStop();
        }

        private void BundlesPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            bundlesPageViewModel.GoToMainPage.Execute(null);
        }
        
        private void BundlesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((BundlesPageViewModel)this.DataContext).GoToTemplatePage.Execute(e.ClickedItem);
        }
    }
}
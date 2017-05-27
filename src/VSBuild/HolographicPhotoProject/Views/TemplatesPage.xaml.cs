// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Models;
using HolographicPhotoProject.ViewModels;
using System;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HolographicPhotoProject
{
    public sealed partial class TemplatesPage : Page
    {
        private App app = Application.Current as App;

        public static class Constants
        {
            public const string TriggerRefresh = "Refresh";
        }

        public TemplatesPage()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += TemplatesPage_BackRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = e.Parameter;

            var templatePageViewModel = (TemplatesPageViewModel)this.DataContext;
            templatePageViewModel.GoToMainPage = new RelayCommand<object>(async param =>
            {
                if (app.MainPageRef != null && app.MainViewId != 0)
                {
                    templatePageViewModel.LoadExistingSouvenirs();
                    templatePageViewModel.SelectedSouvenirModel.TemplatePrefabPath = (param as TemplateModel)?.PrefabPath;
                    templatePageViewModel.SetSelectedSouvenir();
                    await ApplicationViewSwitcher.SwitchAsync(app.MainViewId, app.TemplatesViewId, ApplicationViewSwitchingOptions.ConsolidateViews);
                }
                else
                {
                    Debug.WriteLine("Error: MainPageId is not set or MainPageRef is null.");
                }
            }, (object param) => true);

            templatePageViewModel.GoToBundlesPage = new RelayCommand<object>(async param =>
            {
                if (app.BundlesViewId != 0)
                {
                    await ApplicationViewSwitcher.SwitchAsync(app.BundlesViewId, app.TemplatesViewId, ApplicationViewSwitchingOptions.ConsolidateViews);
                }
                else
                {
                    Debug.WriteLine("Error: Bundles page id is not set.");
                }
            }, (object param) => true);
        }

        private async void TemplatesPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (app.BundlesViewId != 0)
            {
                await ApplicationViewSwitcher.SwitchAsync(app.BundlesViewId, app.TemplatesViewId, ApplicationViewSwitchingOptions.ConsolidateViews);
            }
        }

        private void TemplatesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ((TemplatesPageViewModel)this.DataContext).GoToMainPage.Execute(e.ClickedItem);
        }
    }    
}

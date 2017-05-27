// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Models;
using HolographicPhotoProject.Data.RoamingProfiles;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace HolographicPhotoProject.ViewModels
{
    /// <summary>
    /// View model for the bundles page that handles display of albums.
    /// </summary>
    public class BundlesPageViewModel : FlatPageViewModel<BundleModel>
    {
        private string headingText;
        public string HeadingText
        {
            get { return headingText; }
            set
            {
                var displayName = String.IsNullOrEmpty(value) ? "Your" : $"{value}'s";
                headingText = $"{displayName} OneDrive";
                OnPropertyChanged("HeadingText");
            }
        }

        private bool itemsExist;
        public bool ItemsExist
        {
            get { return itemsExist; }
            set
            {
                itemsExist = value;
                OnPropertyChanged("ItemsExist");
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        public ICommand GoToTemplatePage { get; set; }

        public RelayCommand<object> AuthenticateCommand { get; set; }

        private bool isLoadingAlbums;
        private Object loadAlbumsMutex;
        protected DispatcherTimer dispatcherTimer;

        public BundlesPageViewModel()
        {
            SelectedSouvenirModel = new SouvenirModel();
            AuthenticateCommand = new RelayCommand<object>(AuthenticateCommandExecute, AuthenticateCommandCanExecute);
            loadAlbumsMutex = new Object();
            IsLoading = true;
            Reset();

            DispatchTimerSetup();
        }

        protected void DispatchTimerSetup()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        public void DispatchTimerStart()
        {
            if (!dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Start();
            }
        }

        public void DispatchTimerStop()
        {
            if (dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
            }
        }

        protected async void DispatcherTimer_Tick(object sender, object e)
        {
            var connectionCheck = CheckConnectivity();
            if (HasInternetConnection != connectionCheck)
            {
                HasInternetConnection = connectionCheck;
                await AuthenticateAsync();
                Refresh();
            }
        }

        /// <summary>
        /// Refreshes the bundles page to reload the albums if authenticated, or else clear the bundles page.
        /// </summary>
        public override void Refresh()
        {
            if (IsAuthenticated && HasInternetConnection)
            {
                IsLoading = true;

                LoadAlbumsAsync();
                LoadDisplayName();
            }
            else
            {
                Reset();
            }

            AuthenticateCommand.RaiseCanExecuteChanged();
            SyncCommand.RaiseCanExecuteChanged();
            LogOutCommand.RaiseCanExecuteChanged();
        }

        private async void LoadAlbumsAsync()
        {
            lock (loadAlbumsMutex)
            {
                if (isLoadingAlbums)
                {
                    return;
                }

                isLoadingAlbums = true;
            }

            Items.Clear();
            PageItems.Clear();
            int count = 0;
            var albums = await oneDriveController.GetAlbumsAsync();
            if (albums == null)
            {
                if (!HasInternetConnection)
                {
                    Debug.WriteLine("No internet connection.");
                }
                ItemsExist = false;
                IsLoading = false;

                Debug.WriteLine("Error: Fetching albums from OneDrive returns null.");

                lock (loadAlbumsMutex)
                {
                    isLoadingAlbums = false;
                }
            }
            else
            {
                foreach (var album in albums)
                {
                    var bundle = new BundleModel(album.Id, album.Name, album.ThumbnailUrl);
                    if (count < Constants.NumberOfItemsPerPage)
                    {
                        PageItems.Add(bundle);
                        ++count;
                    }
                    Items.Add(bundle);
                }
            }
           
            PageCursor = 1;
            NumberOfPages = (Items.Count + Constants.NumberOfItemsPerPage - 1) / Constants.NumberOfItemsPerPage;
            NumberOfPages = NumberOfPages < 1 ? 1 : NumberOfPages;
            ItemsExist = Items.Count != 0;

            PageBackCommand.RaiseCanExecuteChanged();
            PageForwardCommand.RaiseCanExecuteChanged();

            IsLoading = false;

            lock (loadAlbumsMutex)
            {
                isLoadingAlbums = false;
            }
        }

        private void LoadDisplayName()
        {
            HeadingText = oneDriveController.GetUserDisplayName();
        }

        private bool AuthenticateCommandCanExecute(object parameter)
        {
            return !IsAuthenticated && HasInternetConnection;
        }

        private async void AuthenticateCommandExecute(object parameter)
        {
            await AuthenticateAsync();
        }

        protected override async void LogOutCommandExecute(object parameter)
        {
            await SignOutAsync();
            UnloadSouvenirs();
            Refresh();
        }

        protected override bool SyncCommandCanExecute(object parameter)
        {
            return IsAuthenticated && HasInternetConnection;
        }

        protected override void SyncCommandExecute(object parameter)
        {
            Refresh();
        }
    }
}
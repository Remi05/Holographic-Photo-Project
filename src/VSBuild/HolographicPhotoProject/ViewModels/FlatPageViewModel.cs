﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Data.RoamingProfiles;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.Networking.Connectivity;

namespace HolographicPhotoProject.ViewModels
{
    /// <summary>
    /// View model for the flat page that handles page navigation.
    /// </summary>
    public abstract class FlatPageViewModel<T> : BaseViewModel
    {
        public ICommand GoToMainPage { get; set; }

        public List<T> Items { get; set; }

        private ObservableCollection<T> pageItems;
        public ObservableCollection<T> PageItems
        {
            get { return pageItems; }
            set
            {
                pageItems = value;
                OnPropertyChanged("PageItems");
            }
        }

        private int pageCursor;
        public int PageCursor
        {
            get { return pageCursor; }
            set
            {
                pageCursor = value;
                OnPropertyChanged("PageCursor");
            }
        }

        private int numberOfPages;
        public int NumberOfPages
        {
            get { return numberOfPages; }
            set
            {
                numberOfPages = value;
                OnPropertyChanged("NumberOfPages");
            }
        }

        private bool hasInternetConnection;
        public bool HasInternetConnection
        {
            get
            {
                return hasInternetConnection;                
            }
            set
            {
                hasInternetConnection = value;
                OnPropertyChanged("HasInternetConnection");
            }
        }

        public SouvenirModel SelectedSouvenirModel { get; set; }

        public RelayCommand<object> LogOutCommand { get; set; }
        public RelayCommand<object> SyncCommand { get; set; }
        public RelayCommand<object> PageBackCommand { get; set; }
        public RelayCommand<object> PageForwardCommand { get; set; }

        public FlatPageViewModel()
        {
            Items = new List<T>();
            PageItems = new ObservableCollection<T>();

            LogOutCommand = new RelayCommand<object>(LogOutCommandExecute, LogOutCommandCanExecute);
            SyncCommand = new RelayCommand<object>(SyncCommandExecute, SyncCommandCanExecute);
            PageBackCommand = new RelayCommand<object>(PageBackCommandExecute, PageBackCommandCanExecute);
            PageForwardCommand = new RelayCommand<object>(PageForwardCommandExecute, PageForwardCommandCanExecute);

            HasInternetConnection = CheckConnectivity();
        }

        protected bool CheckConnectivity()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            return connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
        }

        private bool LogOutCommandCanExecute(object parameter)
        {
            return IsAuthenticated && HasInternetConnection;
        }

        /// <summary>
        /// By default, the log out button simply signs out.
        /// To add additional behaviour to this button, an inheriting view model must override this method.
        /// </summary>
        protected virtual async void LogOutCommandExecute(object parameter)
        {
            await SignOutAsync();
        }

        /// <summary>
        /// By default, the sync button is not active.
        /// </summary>
        protected virtual bool SyncCommandCanExecute(object parameter)
        {
            return false;
        }

        /// <summary>
        /// By default, the sync button does nothing.
        /// To add behaviour to this button, an inheriting view model must override this method.
        /// </summary>
        protected virtual void SyncCommandExecute(object parameter)
        {
            return;
        }

        private bool PageBackCommandCanExecute(object parameter)
        {
            return PageCursor > 1;
        }

        private void PageBackCommandExecute(object parameter)
        {
            PageItems.Clear();
            --PageCursor;
            int startIndex = (PageCursor - 1) * Constants.NumberOfItemsPerPage;
            for (var i = startIndex; i < startIndex + Constants.NumberOfItemsPerPage; ++i)
            {
                if (i >= Items.Count)
                {
                    break;
                }

                PageItems.Add(Items[i]);
            }
            PageBackCommand.RaiseCanExecuteChanged();
            PageForwardCommand.RaiseCanExecuteChanged();
        }

        private bool PageForwardCommandCanExecute(object parameter)
        {
            return PageCursor * Constants.NumberOfItemsPerPage < Items.Count;
        }

        private void PageForwardCommandExecute(object parameter)
        {
            PageItems.Clear();
            ++PageCursor;
            int startIndex = (PageCursor - 1) * Constants.NumberOfItemsPerPage;
            for (var i = startIndex; i < startIndex + Constants.NumberOfItemsPerPage; ++i)
            {
                if (i >= Items.Count)
                {
                    break;
                }
                PageItems.Add(Items[i]);
            }
            PageBackCommand.RaiseCanExecuteChanged();
            PageForwardCommand.RaiseCanExecuteChanged();
        }

        protected void Reset()
        {
            PageCursor = 1;
            NumberOfPages = 1;
            Items.Clear();
            PageItems.Clear();

            PageBackCommand.RaiseCanExecuteChanged();
            PageForwardCommand.RaiseCanExecuteChanged();
        }
    }
}

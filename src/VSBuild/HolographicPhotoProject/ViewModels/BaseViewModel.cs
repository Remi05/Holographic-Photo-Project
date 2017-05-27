// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Data.Onedrive.Controllers;
using HolographicPhotoProject.Souvenirs;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityPlayer;

namespace HolographicPhotoProject.ViewModels
{
    /// <summary>
    /// Base view model for the app that handles authentication.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected OneDriveController oneDriveController = OneDriveController.Instance;

        public static class Constants
        {
            /// <summary>
            /// Path prefix for the 2D thumbnails of the 3D model templates.
            /// </summary>
            public const string TemplateThumbnailRoot = "Assets/Souvenirs/";
            public const int NumberOfItemsPerPage = 8;
        }

        /// <summary>
        /// Indicates whether or not the user is authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                OnPropertyChanged("IsAuthenticated");
                return oneDriveController.IsAuthenticated();
            }
        }

        /// <summary>
        /// Attempts to authenticate the user to OneDrive and refresh the page.
        /// </summary>
        public virtual async Task AuthenticateAsync()
        {
            if (!IsAuthenticated)
            {
                await oneDriveController.AuthenticateAsync();
            }
            Refresh();
        }

        /// <summary>
        /// Attempts to sign the user out of OneDrive and refresh the page.
        /// </summary>
        /// <returns></returns>
        public async Task SignOutAsync()
        {
            if (IsAuthenticated)
            {
                await oneDriveController.SignOutAsync();
            }
            Refresh();
        }

        /// <summary>
        /// Loads the souvenirs of the current user into the environment.
        /// </summary>
        public void LoadExistingSouvenirs()
        {
            if (AppCallbacks.Instance.IsInitialized())
            {
                AppCallbacks.Instance.InvokeOnAppThread(() =>
                {
                    var souvenirManager = UnityEngine.Object.FindObjectOfType<SouvenirManager>();
                    souvenirManager.TryPlaceSouvenirs();
                }, false);
            }
        }

        /// <summary>
        /// Unloads the currently loaded souvenirs from the environment.
        /// </summary>
        public void UnloadSouvenirs()
        {
            if (AppCallbacks.Instance.IsInitialized())
            {
                AppCallbacks.Instance.InvokeOnAppThread(() =>
                {
                    var souvenirManager = UnityEngine.Object.FindObjectOfType<SouvenirManager>();
                    souvenirManager.TryUnloadSouvenirs();
                }, false);
            }
        }

        /// <summary>
        /// Refresh view model data, if necessary, after authentication. Does nothing by default.
        /// </summary>
        public virtual void Refresh() { }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

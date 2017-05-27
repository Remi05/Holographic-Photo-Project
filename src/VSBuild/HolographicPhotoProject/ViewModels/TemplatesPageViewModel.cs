// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Models;
using HolographicPhotoProject.Souvenirs;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using UnityPlayer;

namespace HolographicPhotoProject.ViewModels
{
    /// <summary>
    /// View model for the templates page that handles display of template thumbnails.
    /// </summary>
    public class TemplatesPageViewModel : FlatPageViewModel<TemplateModel>
    {
        public ICommand GoToBundlesPage { get; set; }

        public TemplatesPageViewModel()
        {
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            Items.Clear();
            PageItems.Clear();
            int count = 0;
            string[] files = Directory.GetFiles(Constants.TemplateThumbnailRoot);
            if (files == null)
            {
                Debug.Write("Error: Reading files from the souvenirs image folder returns null.");
                return;
            }
            foreach (var file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file);
                TemplateModel template = new TemplateModel(name, Path.GetFullPath(file), name);
                if (count < Constants.NumberOfItemsPerPage)
                {
                    PageItems.Add(template);
                    ++count;
                }
                Items.Add(template);
            }
            PageCursor = 1;
            NumberOfPages = (Items.Count + Constants.NumberOfItemsPerPage - 1) / Constants.NumberOfItemsPerPage;
            PageBackCommand.RaiseCanExecuteChanged();
            PageForwardCommand.RaiseCanExecuteChanged();
        }
        
        public void SetSelectedSouvenir()
        {
            string id = Guid.NewGuid().ToString();
            SelectedSouvenirModel.Id = id;

            if (AppCallbacks.Instance.IsInitialized())
            {
                AppCallbacks.Instance.TryInvokeOnAppThread(() =>
                {
                    var souvenirManager = UnityEngine.Object.FindObjectOfType<SouvenirManager>();
                    if (souvenirManager != null)
                    {
                        souvenirManager.CreateSouvenir(SelectedSouvenirModel.Id,
                                                       SelectedSouvenirModel.TemplatePrefabPath,
                                                       SelectedSouvenirModel.BundleId,
                                                       isNewSouvenir: true);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("Could not find SouvenirManager object in the scene.");
                    }
                }, false);
            }
        }

        protected override async void LogOutCommandExecute(object parameter)
        {
            await SignOutAsync();
            UnloadSouvenirs();
            GoToBundlesPage.Execute(null);
        }  
    }
}

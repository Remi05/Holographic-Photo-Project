// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.IO;
using System.Linq;
using System;
#if UNITY_UWP
using Microsoft.OneDrive.Sdk;
#endif

namespace HolographicPhotoProject.Data.Onedrive.Models
{
    /// <summary>
    /// Represents a OneDrive item.
    /// </summary>
#if UNITY_UWP
    public class OneDriveItemModel : IItemModel
    {
        private MemoryStream memoryStream;
        public OneDriveItemModel(Item item)
        {
            Item = item;
        }

        public OneDriveItemModel(string id)
        {
            Item = new Item();
            Item.Id = id;
        }

        public Item Item { get; private set; }

        /// <summary>
        /// Id number of the OneDrive item.
        /// </summary>
        public string Id => Item?.Id;

        /// <summary>
        /// File name of the OneDrive item.
        /// </summary>
        public string Name => Item?.Name;

        /// <summary>
        /// Content stream for the OneDrive item image.
        /// </summary>
        public MemoryStream MemoryStream
        {
            get
            {
                return memoryStream;
            }
            set
            {
                memoryStream = value;
                OnPropertyChanged("MemoryStream");
            }
        }

        /// <summary>
        /// Url for the thumbnail of the OneDrive item image.
        /// </summary>
        public string ThumbnailUrl => Item?.Thumbnails?.FirstOrDefault()?.Large?.Url ?? string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
#else
    public class OneDriveItemModel : IItemModel
    {
        /// <summary>
        /// Id of the item.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content stream for the image.
        /// </summary>
        public MemoryStream MemoryStream { get; set; }

        public string ThumbnailUrl { get; set; }

    #pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
    #pragma warning restore 0067
    }
#endif
}
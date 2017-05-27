// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;

namespace HolographicPhotoProject.Models
{
    /// <summary>
    /// Model for the bundle, which serves as the content source.
    /// </summary>
    public class BundleModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        private string id;
        /// <summary>
        /// Id of the item.
        /// </summary>
        public string Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
        
        private string name;
        /// <summary>
        /// Name of the item.
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        
        private string thumbnailUrl;
        /// <summary>
        /// Url for the thumbnail.
        /// </summary>
        public string ThumbnailUrl
        {
            get { return thumbnailUrl; }
            set
            {
                thumbnailUrl = value;
                OnPropertyChanged("ThumbnailUrl");
            }
        }
        
        public BundleModel(string id, string name, string thumbnailUrl)
        {
            Id = id;
            Name = name;
            ThumbnailUrl = thumbnailUrl;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

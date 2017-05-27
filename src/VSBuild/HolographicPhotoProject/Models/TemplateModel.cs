// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;

namespace HolographicPhotoProject.Models
{
    /// <summary>
    /// Model for the template, which is the 3D model prefab for the hologram.
    /// </summary>
    public class TemplateModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        private string thumbnailPath;
        /// <summary>
        /// Path for the thumbnail.
        /// </summary>
        public string ThumbnailPath
        {
            get { return thumbnailPath; }
            set
            {
                thumbnailPath = value;
                OnPropertyChanged("ThumbnailPath");
            }
        }
        
        /// <summary>
        /// Path for the prefab of the 3D model.
        /// </summary>
        public string PrefabPath { get; private set; }

        public TemplateModel(string name, string thumbnailPath, string prefabPath)
        {
            Name = name;
            ThumbnailPath = thumbnailPath;
            PrefabPath = prefabPath;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

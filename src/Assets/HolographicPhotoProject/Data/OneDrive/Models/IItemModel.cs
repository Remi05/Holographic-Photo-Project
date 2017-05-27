// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.IO;

namespace HolographicPhotoProject.Data.Onedrive.Models
{
    public interface IItemModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Id of the item.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Name of the item.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Content stream for the image.
        /// </summary>
        MemoryStream MemoryStream { get; set; }
        /// <summary>
        /// Url for the thumbnail of the OneDrive item image.
        /// </summary>
        string ThumbnailUrl { get; }
    }
}
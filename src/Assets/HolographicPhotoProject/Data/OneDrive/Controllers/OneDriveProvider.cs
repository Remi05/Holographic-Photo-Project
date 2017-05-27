// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Presenters.Utilities;
using HolographicPhotoProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_UWP
using System.Threading.Tasks;
#endif

namespace HolographicPhotoProject.Data.Onedrive.Controllers
{
    /// <summary>
    /// Implementation of the provider for OneDrive. 
    /// </summary>
    public class OneDriveProvider : ContentProvider
    {
        private int accessorCount = 0;
        private OneDriveController oneDriveController = OneDriveController.Instance;
        private string parentItemId = null;

        /// <summary>
        /// Whether the content providers contains any resource or not.
        /// </summary>
        protected override bool IsEmpty
        {
            get { return NumberOfResources == 0; }
        }

        /// <summary>
        /// Number of resources in the content provider.
        /// </summary>
        protected override int NumberOfResources
        {
            get { return ResourceIds == null ? 0 : ResourceIds.Count; }
        }

        private IList<string> resourceIds;
        private IList<string> ResourceIds
        {
            get { return resourceIds; }
            set
            {
                resourceIds = value;
            }
        }

        /// <param name="parentItemId">ID of the parent OneDrive item (folder/album/bundle).</param>
        public OneDriveProvider(string parentItemId)
        {
            this.parentItemId = parentItemId;
            if (EventQueueManager.Instance == null)
            {
                EventQueueManager.Init();
            }
        }

        /// <summary>
        /// Gets the ID of the resource at the given
        /// index wrapped within the ID collection or
        /// null if the collection is empty/null.
        /// </summary>
        private string GetResourceIdAt(int index)
        {
            return IsEmpty ? null : resourceIds[WrapIndex(index)];
        }

        /// <summary>
        /// Returns a new accessor to the content.
        /// </summary>
        public override IContentAccessor GetNewAccessor()
        {
            // Move up (and wrap) the start index for the new accessor by one 
            // each time so that not all presenters start with the same resource.
            return new ContentAccessor(this, accessorCount++);
        }

#if UNITY_UWP
        /// <summary>
        /// Fetches the IDs of the children resources from OneDrive.
        /// </summary>
        /// <param name="forceRefresh">Whether the children IDs should be fetched from OneDrive again or not.</param>
        private async Task FetchResourceIds(bool forceRefresh = false)
        {
            if (parentItemId != null && (ResourceIds == null || forceRefresh))
            {
                // Should check if each child is a resource and not another container (folder/album/bundle)...
                IEnumerable<string> children = await oneDriveController.GetChildrenAsync(parentItemId);
                if (children == null)
                {
                    Debug.LogError("Failed to fetch the parent item's children.");
                    return;
                }
                ResourceIds = children.ToList();
            }
        }

        /// <summary>
        /// Fetches the resource at the given index from OneDrive, formats it into the
        /// requested type using the given ContentFormatter and returns it to the callback function.
        /// </summary>
        /// <typeparam name="T">Type of the resource to create with the fetched data.</typeparam>
        /// <param name="formatter">Function to use to convert the raw data into a resource of type T.</param>
        /// <param name="callback">Function to which the created resource should be sent.</param>
        protected override async Task GetResourceAt<T>(int resourceIndex, ContentFormatter<T> formatter, Action<T> callback)
        {
            // We fetch the children for the first time
            // here since we can't do it in the constructor.
            if (ResourceIds == null)
            {
                await FetchResourceIds();
            }

            string resourceId = GetResourceIdAt(resourceIndex);
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                Debug.LogError("The requested resource has a null ID.");
                return;
            }

            byte[] resourceData = await oneDriveController.GetItemContentAsync(resourceId);

            // Decompress bytes on a background thread.
            ImageUtils.ImageInfo imageInfo = await ImageUtils.DecompressImageBytes(resourceData);

            if (imageInfo.DecompressedBytes == null)
            {
                Debug.LogError("Failed to fetch the requested resource from OneDrive.");
                return;
            }

            // EventQueueManager executes the given Action on the main thread.
            // This is required for resource formatting (i.e. Texture2D.LoadImage()).
            EventQueueManager.Instance?.Execute(() =>
            {
                T resource = formatter(imageInfo);
                callback(resource);
            });
        }
#endif
    }
}
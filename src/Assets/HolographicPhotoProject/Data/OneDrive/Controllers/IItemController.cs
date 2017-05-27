// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Data.Onedrive.Models;
using System.Collections.Generic;
#if UNITY_UWP
using System.Threading.Tasks;

namespace HolographicPhotoProject.Data.Onedrive.Controllers
{
    public interface IHSItemController
    {
        /// <summary>
        /// Authenticate user to service.
        /// </summary>
        Task AuthenticateAsync();

        /// <summary>
        /// Indicates if the user is currently authenticated to service or not.
        /// </summary>
        bool IsAuthenticated();

        /// <summary>
        /// Signs user out of service.
        /// </summary>
        Task SignOutAsync();

        /// <summary>
        /// Retrieve item given an item ID.
        /// </summary>
        Task<IItemModel> GetItemAsync(string itemId);

        /// <summary>
        /// Retrieve image content stream given an item.
        /// </summary>
        Task SetImageContentAsync(IItemModel item);

        /// <summary>
        /// Retrieves children IDs given an item.
        /// </summary>
        Task<IEnumerable<string>> GetChildrenAsync(IItemModel item);
    }
}
#else
namespace HolographicPhotoProject.Data.Onedrive.Controllers
{
    public interface IHSItemController
    {
        /// <summary>
        /// Authenticate user to service.
        /// </summary>
        void Authenticate();

        /// <summary>
        /// Indicates if the user is currently authenticated to service or not.
        /// </summary>
        bool IsAuthenticated();

        /// <summary>
        /// Signs user out of service.
        /// </summary>
        void SignOut();

        /// <summary>
        /// Retrieve item given an item ID.
        /// </summary>
        IItemModel GetItem(string itemId);

        /// <summary>
        /// Retrieve image content stream given an item.
        /// </summary>
        void SetImageContent(IItemModel item);

        /// <summary>
        /// Retrieves children IDs given an item.
        /// </summary>
        Queue<string> GetChildren(IItemModel item);
    }
}
#endif
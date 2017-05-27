// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_UWP
using Microsoft.Graph;
using Microsoft.OneDrive.Sdk;
using Microsoft.OneDrive.Sdk.Authentication;
using HolographicPhotoProject.Data.Onedrive.Models;
using HolographicPhotoProject.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HolographicPhotoProject.Data.Onedrive.Controllers
{
    /// <summary>
    /// Controller that allows authentication and data retrieval from OneDrive.
    /// </summary>
    public sealed class OneDriveController : MultiThreadedSingleton<OneDriveController>
    {
        private MsaAuthenticationProvider authProvider;
        private OneDriveClient oneDriveClient = null;
        private Drive drive = null;

        public OneDriveController()
        {
            authProvider = new MsaAuthenticationProvider(ConfigurationManager.ClientId, ConfigurationManager.ReturnUrl, ConfigurationManager.Scopes, null, new CredentialVault(ConfigurationManager.ClientId));
        }

        /// <summary>
        /// Authenticate user to OneDrive.
        /// </summary>
        public async Task AuthenticateAsync()
        {
            try
            {
                oneDriveClient = new OneDriveClient("https://api.onedrive.com/v1.0", authProvider);
                await authProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
                await SetDriveAsync();
            }
            catch (ServiceException e)
            {
                Debug.WriteLine(e?.Error?.Message);
                oneDriveClient = null;
            }
        }

        /// <summary>
        /// Indicates if the user is currently authenticated to OneDrive or not.
        /// </summary>
        public bool IsAuthenticated()
        {
            return oneDriveClient != null;
        }

        /// <summary>
        /// Signs user out of OneDrive.
        /// </summary>
        public async Task SignOutAsync()
        {
            try
            {
                await authProvider.SignOutAsync();
                drive = null;
            }
            finally
            {
                oneDriveClient = null;
            }
        }

        /// <summary>
        /// Set up the Drive instance in the class which can access the user information.
        /// </summary>
        private async Task SetDriveAsync()
        {
            try
            {
                drive = await oneDriveClient?.Drive
                                            ?.Request()
                                            .GetAsync();
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is ServiceException)
                    {
                        Debug.WriteLine(x.StackTrace);
                        return true;
                    }
                    return false;
                });
            }
        }

        /// <summary>
        /// Returns signed-in user's display name.
        /// </summary>
        public string GetUserDisplayName()
        {
            return drive?.Owner?.User?.DisplayName ?? String.Empty;
        }

        /// <summary>
        /// Returns signed-in user's ID.
        /// </summary>
        public string GetUserId()
        {
            return drive?.Owner?.User?.Id ?? String.Empty;
        }

        /// <summary>
        /// Retrieve item from OneDrive given an item ID.
        /// </summary>
        public async Task<IItemModel> GetItemAsync(string itemId)
        {
            Item item = null;
            try
            {
                item = await oneDriveClient?.Drive
                                           ?.Items[itemId]
                                           ?.Request()
                                           .GetAsync();
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is ServiceException)
                    {
                        Debug.WriteLine(x.StackTrace);
                        return true;
                    }
                    return false;
                });
            }

            return item == null ? null : new OneDriveItemModel(item);
        }

        /// <summary>
        /// Retrieves the content of the requested item and returns it as a memory stream.
        /// </summary>
        public async Task<MemoryStream> GetItemContentStreamAsync(string itemId)
        {
            var itemContentStream = await oneDriveClient?.Drive
                                                        ?.Items[itemId]
                                                        ?.Content
                                                        ?.Request()
                                                        .GetAsync();

            return itemContentStream as MemoryStream;
        }

        /// <summary>
        /// Retrieves the content of the requested item and returns it as a byte array.
        /// </summary>
        public async Task<byte[]> GetItemContentAsync(string itemId)
        {
            var itemContentStream = await GetItemContentStreamAsync(itemId);
            return itemContentStream?.ToArray();
        }

        /// <summary>
        /// Retrieves children IDs from OneDrive given an item ID.
        /// </summary>
        public async Task<IEnumerable<string>> GetChildrenAsync(string itemId)
        {
            var children = await oneDriveClient?.Drive
                                               ?.Items[itemId]
                                               ?.Children
                                               ?.Request()
                                               .GetAsync();

            return children?.CurrentPage?.Where(child => child.Image != null).Select(child => child.Id) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Retrieves children IDs from OneDrive given an item.
        /// </summary>
        public async Task<IEnumerable<string>> GetChildrenAsync(IItemModel item)
        {
            return await GetChildrenAsync(item.Id);
        }

        /// <summary>
        /// Retrieves albums from OneDrive.
        /// </summary>
        public async Task<List<OneDriveItemModel>> GetAlbumsAsync()
        {
            var bundleUrl = oneDriveClient?.Drive
                                          ?.AppendSegmentToRequestUrl("bundles");
            List<OneDriveItemModel> items = new List<OneDriveItemModel>();
            try
            {
                var albums = await new DriveItemsCollectionRequestBuilder(bundleUrl, oneDriveClient)?.Request()
                                                                                          .Filter("bundle/album ne null")
                                                                                          .Expand("thumbnails")
                                                                                          .GetAsync();
                foreach (var album in albums)
                {
                    items.Add(new OneDriveItemModel(album));
                }
            }
            catch (ServiceException e)
            {
                Debug.WriteLine(e?.Error?.Message);
                return null;
            }
            // Sort albums by creation date (starting from most recent)
            items.Sort((x, y) => Nullable.Compare(y.Item.CreatedDateTime, x.Item.CreatedDateTime));
            return items;
        }
    }
}
#else
using HolographicPhotoProject.Data.Onedrive.Models;
using HolographicPhotoProject.Utilities;
using System;
using System.Collections.Generic;

namespace HolographicPhotoProject.Data.Onedrive.Controllers
{
    /// <summary>
    /// Controller than allowes to authenticate and fetch data on OneDrive.
    /// </summary>
    public sealed class OneDriveController : MultiThreadedSingleton<OneDriveController>, IHSItemController
    {
        /// <summary>
        /// Authenticate user to service.
        /// </summary>
        public void Authenticate()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates if the user is currently authenticated to service or not.
        /// </summary>
        public bool IsAuthenticated()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Signs user out of service.
        /// </summary>
        public void SignOut()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve item given an item ID.
        /// </summary>
        public IItemModel GetItem(string itemId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieve image content stream given an item.
        /// </summary>
        public void SetImageContent(IItemModel item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves children IDs given an item.
        /// </summary>
        public Queue<string> GetChildren(IItemModel item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves albums from OneDrive.
        /// </summary>
        public void GetAlbums()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// Contains the configuration of the app.
    /// </summary>
    public static class ConfigurationManager
    {
        public const string ClientId = "INSERT YOUR OWN CLIENT ID HERE";
        public const string ReturnUrl = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        public static readonly string[] Scopes = { "onedrive.readonly", "wl.photos" };
    }
}

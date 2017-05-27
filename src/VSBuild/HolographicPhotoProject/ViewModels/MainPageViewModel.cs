// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using System.Windows.Input;

namespace HolographicPhotoProject.ViewModels
{
    /// <summary>
    /// View model for the main page that handles authentication and loading the souvenirs.
    /// </summary>
    public class MainPageViewModel : BaseViewModel
    {
        public override async Task AuthenticateAsync()
        {
            await base.AuthenticateAsync();

            if (IsAuthenticated)
            {
                LoadExistingSouvenirs();
            }
        }
    }
}

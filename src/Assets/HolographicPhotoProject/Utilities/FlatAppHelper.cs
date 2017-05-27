// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// Opens up the flat app from the 3D environment.
    /// </summary>
    public static class FlatAppHelper
    {
        public delegate void NavigateToFlatDelegate();

        private static NavigateToFlatDelegate navigateToFlatApp;
        /// <summary>
        /// Delegate used to navigate to the flat app (needs to be set in flat app first).
        /// </summary>
        public static NavigateToFlatDelegate NavigateToFlatApp
        {
            // "get" returns a lambda which calls the 
            // "navigateToFlatApp" delegate on the UI thread.
            get
            {
                return () =>
                {
                    // The delegate needs to be invoked on the UI thread
                    // to have the right to open the flat app window.
                    UnityEngine.WSA.Application.InvokeOnUIThread(() =>
                    {
                        if (navigateToFlatApp != null)
                        {
                            navigateToFlatApp();
                        }
                    }, false);
                };
            }
            set
            {
                navigateToFlatApp = value;
            }
        }
    }
}
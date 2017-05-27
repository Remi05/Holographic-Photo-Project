// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.UI.Onboarding
{
    public class MenuPinRotate : MonoBehaviour
    {
        private static class Constants
        {
            /// <summary>
            /// The speed at which the menu pin rotates.
            /// </summary>
            public static Vector3 MenuPinRotateSpeed = new Vector3(0f, 0.5f, 0f);
        }

        private void Update()
        {
            transform.Rotate(Constants.MenuPinRotateSpeed);
        }
    }
}
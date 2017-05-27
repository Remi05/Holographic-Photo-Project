// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.Utilities
{
    public class Billboard : MonoBehaviour
    {
        private static class Constants
        {
            // Rotation adjustment used so that the plane is perpendicular to the camera instead of parallel.
            public static readonly Vector3 DefaultRotationAdjustment = new Vector3(90.0f, 0.0f, 0.0f);
        }

        /// <summary>
        /// Rotates the GameObject to face the main camera.
        /// </summary>
        private void Update()
        {
            if (Camera.main == null)
            {
                return;
            }

            gameObject.transform.LookAt(Camera.main.transform.position);
            gameObject.transform.Rotate(Constants.DefaultRotationAdjustment);
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// A collection of helper functions related to transforms.
    /// </summary>
    public static class TransformsHelper
    {
        /// <summary>
        /// Transforms a scale from world to local coordinates.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="worldScale"></param>
        /// <returns></returns>
        public static Vector3 WorldToLocalScale(Transform transform, Vector3 worldScale)
        {
            var initialScale = transform.localScale;
            transform.localScale = Vector3.one;

            var transformedScale = new Vector3(worldScale.x / transform.lossyScale.x,
                                               worldScale.y / transform.lossyScale.y,
                                               worldScale.z / transform.lossyScale.z);
            transform.localScale = initialScale;

            return transformedScale;
        }
    }
}
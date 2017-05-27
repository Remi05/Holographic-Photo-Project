// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Utilities;
using UnityEngine;

#if UNITY_UWP
using Windows.Graphics.Imaging;
#endif

namespace HolographicPhotoProject.Presenters.Utilities
{
    /// <summary>
    /// Function which receives a byte array and returns a resource of type T.
    /// </summary>
    /// <typeparam name="T">The type of the resource to create.</typeparam>
    /// <param name="resourceData">The raw resource data.</param>
    /// <returns>The created resource of type T.</returns>
    public delegate T ContentFormatter<T>(ImageUtils.ImageInfo imageInfo);

    /// <summary>
    /// Static class containing common ContentFormatters.
    /// </summary>
    public static class ContentFormatters
    {
        /// <summary>
        /// Creates and returns a new Texture2D using the given raw image data.
        /// </summary>
        public static Texture2D TextureFormatter(ImageUtils.ImageInfo imageInfo)
        {
#if UNITY_UWP
            TextureFormat imageFormat;
            switch (imageInfo.Format)
            {
                case BitmapPixelFormat.Bgra8:
                    imageFormat = TextureFormat.BGRA32;
                    break;
                case BitmapPixelFormat.Rgba8:
                    imageFormat = TextureFormat.RGBA32;
                    break;
                default:
                    // Reject image on unknown format.
                    // We could also return a placeholder image.
                    return null;
            }
#else
            TextureFormat imageFormat = TextureFormat.BGRA32;
#endif

            Texture2D image = new Texture2D(imageInfo.Width, imageInfo.Height, imageFormat, false);
            image.LoadRawTextureData(imageInfo.DecompressedBytes);
            image.Apply();

            return image;
        }
    }
}

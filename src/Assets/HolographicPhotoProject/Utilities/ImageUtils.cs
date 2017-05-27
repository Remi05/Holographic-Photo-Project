// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

#if UNITY_UWP
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#endif

namespace HolographicPhotoProject.Utilities
{
    public static class ImageUtils
    {
        private static class Constants
        {
            /// <summary>
            /// Max image size limit for width or height.
            /// </summary>
            /// If the size is a power of 2, that's faster for the Unity Texture Sampler to read.
            public const int MaxImageSizeLimit = 1024;
        }

        public struct ImageInfo
        {
            public byte[] DecompressedBytes;
            public int Width;
            public int Height;
#if UNITY_UWP
            public BitmapPixelFormat Format;

            public ImageInfo(byte[] decompressedBytes, int width, int height, BitmapPixelFormat format)
            {
                DecompressedBytes = decompressedBytes;
                Width = width;
                Height = height;
                Format = format;
            }
#endif
        }

        /// <summary>
        /// Resizes the input image dimensions to be less than or equal to the given dimension limits.
        /// </summary>
        /// <param name="widthLimit">Max allowed width for the input image.</param>
        /// <param name="heightLimit">Max allowed height for the input image.</param>
        /// <param name="srcImgWidth">Source image width.</param>
        /// <param name="srcImgHeight">Source image height.</param>
        public static void LimitImageDimensionsTo(float widthLimit, float heightLimit, ref float srcImgWidth, ref float srcImgHeight)
        {
            float widthRatio = widthLimit / srcImgWidth;
            float heightRatio = heightLimit / srcImgHeight;

            float minRatio = Math.Min(widthRatio, heightRatio);

            if (minRatio <= 1)
            {
                srcImgWidth *= minRatio;
                srcImgHeight *= minRatio;
            }
        }

#if UNITY_UWP
        /// <summary>
        /// Decompresses the fetched image bytes and returns a `BmpImageInfo` containing
        /// the width, height, pixel format and the uncompressed byte array.
        /// NOTE: There's an assumption here that the bytes will be valid image bytes
        /// </summary>
        public static async Task<ImageUtils.ImageInfo> DecompressImageBytes(byte[] compressedImageBytes)
        {
            IRandomAccessStream stream = new MemoryStream(compressedImageBytes).AsRandomAccessStream();
            if (stream == null)
            {
                Debug.LogError("Byte stream is null.");
                return new ImageUtils.ImageInfo();
            }

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            if (decoder == null)
            {
                Debug.LogError("Bitmap decoder is null.");
                return new ImageUtils.ImageInfo();
            }

            // Figure out the non-oriented image dimensions and clamp to save on memory.
            float defaultWidth = decoder.PixelWidth;
            float defaultHeight = decoder.PixelHeight;
            ImageUtils.LimitImageDimensionsTo(
                Constants.MaxImageSizeLimit,
                Constants.MaxImageSizeLimit,
                ref defaultWidth,
                ref defaultHeight);
            
            BitmapTransform transform = new BitmapTransform();
            transform.ScaledWidth = (uint)defaultWidth;
            transform.ScaledHeight = (uint)defaultHeight;

            // Apply the transform and exif orientation.
            PixelDataProvider pixelProvider = await decoder.GetPixelDataAsync(
                decoder.BitmapPixelFormat,
                decoder.BitmapAlphaMode,
                transform,
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage);

            if (pixelProvider == null)
            {
                Debug.LogError("PixelProvider is null.");
                return new ImageUtils.ImageInfo();
            }

            byte[] decompressedBytes = pixelProvider.DetachPixelData();

            // Because the image is now oriented correctly, before we pass it to Unity,
            // we have to give Unity the correct oriented image dimensions.
            float orientedWidth = decoder.OrientedPixelWidth;
            float orientedHeight = decoder.OrientedPixelHeight;
            ImageUtils.LimitImageDimensionsTo(
                Constants.MaxImageSizeLimit,
                Constants.MaxImageSizeLimit,
                ref orientedWidth,
                ref orientedHeight);

            return new ImageUtils.ImageInfo(
                decompressedBytes,
                (int)orientedWidth,
                (int)orientedHeight,
                decoder.BitmapPixelFormat);
        }
#endif
    }
}
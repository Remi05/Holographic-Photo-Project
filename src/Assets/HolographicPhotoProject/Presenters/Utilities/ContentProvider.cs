// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Presenters.Presenters;
using System;
#if UNITY_UWP
using System.Threading.Tasks;
#endif

namespace HolographicPhotoProject.Presenters.Utilities
{
    public abstract class ContentProvider
    {
        /// <summary>
        /// Content accessor implementation for a generic content provider.
        /// </summary>
        protected class ContentAccessor : IContentAccessor
        {
            private ContentProvider contentProvider;

            private int currentResourceIndex;
            
            public ContentAccessor(ContentProvider contentProvider, int startIndex)
            {
                this.contentProvider = contentProvider;
                currentResourceIndex = startIndex;
            }

#if UNITY_UWP
            public async Task GetCurrentResource<T>(ContentFormatter<T> formatter, Action<T> callback)
            {
                await contentProvider.GetResourceAt<T>(currentResourceIndex, formatter, callback);
            }

            public async Task GetNextResource<T>(ContentFormatter<T> formatter, Action<T> callback, int offset = 1)
            {
                MoveForward(offset);
                await GetCurrentResource<T>(formatter, callback);
            }

            public async Task GetPreviousResource<T>(ContentFormatter<T> formatter, Action<T> callback, int offset = 1)
            {
                MoveBack(offset);
                await GetCurrentResource<T>(formatter, callback);
            }
#endif

            public void MoveForward(int count = 1)
            {
                currentResourceIndex += count;
            }

            public void MoveBack(int count = 1)
            {
                currentResourceIndex -= count;
            }
        }

        /// <summary>
        /// Whether the content providers contains any resource or not.
        /// </summary>
        protected abstract bool IsEmpty { get; }

        /// <summary>
        /// Number of resources in the content provider.
        /// </summary>
        protected abstract int NumberOfResources { get; }

        /// <summary>
        /// Returns a new content accessor.
        /// </summary>
        /// <returns>The newly created content accessor.</returns>
        public abstract IContentAccessor GetNewAccessor();

        /// <summary>
        /// Wraps the given index within the ID collection
        /// (returns 0 if the collection is empty/null).
        /// </summary>
        protected int WrapIndex(int resourceIndex)
        {
            // WrappedMod() wraps the index within the range [0, NumberOfResources-1].
            return IsEmpty ? 0 : WrappedMod(resourceIndex, NumberOfResources);
        }

        /// <summary>
        /// Modulus which wraps negative dividends to
        /// positive integers if the divisor is positive.
        /// </summary>
        /// <param name="n">Dividend.</param>
        /// <param name="m">Divisor.</param>
        /// <returns></returns>
        protected static int WrappedMod(int n, int m)
        {
            return ((n % m) + m) % m;
        }

#if UNITY_UWP
        /// <summary>
        /// Fetches the resource at the given index from a specified
        /// source (implementation specific), formats it into the
        /// requested type using the given ContentFormatter and
        /// returns it to the callback function.
        /// </summary>
        /// <typeparam name="T">Type of the resource to create with the fetched data.</typeparam>
        /// <param name="formatter">Function to use to convert the raw data into a resource of type T.</param>
        /// <param name="callback">Function to which the created resource should be sent.</param>
        protected abstract Task GetResourceAt<T>(int resourceIndex, ContentFormatter<T> formatter, Action<T> callback);
#endif
    }
}

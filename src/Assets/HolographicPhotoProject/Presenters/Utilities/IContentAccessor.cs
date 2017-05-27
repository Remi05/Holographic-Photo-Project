// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_UWP
using HolographicPhotoProject.Presenters.Presenters;
using System;
using System.Threading.Tasks;
#endif

namespace HolographicPhotoProject.Presenters.Utilities
{
    public interface IContentAccessor
    {
#if UNITY_UWP
        /// <summary>
        /// Fetches the current resource from a specified
        /// source (implementation specific), formats it into the
        /// requested type using the given ContentFormatter and
        /// returns it to the callback function.
        /// </summary>
        /// <typeparam name="T">Type of the resource to create with the fetched data.</typeparam>
        /// <param name="formatter">Function to use to convert the raw data into a resource of type T.</param>
        /// <param name="callback">Function to which the created resource should be sent.</param>
        Task GetCurrentResource<T>(ContentFormatter<T> formatter, Action<T> callback);

        /// <summary>
        /// Fetches the resource located delta resources after the current one
        /// from a specified source (implementation specific), formats it into the
        /// requested type using the given ContentFormatter and
        /// returns it to the callback function.
        /// </summary>
        /// <typeparam name="T">Type of the resource to create with the fetched data.</typeparam>
        /// <param name="formatter">Function to use to convert the raw data into a resource of type T.</param>
        /// <param name="callback">Function to which the created resource should be sent.</param>
        /// <param name="offset">Distance to the next resource to be fetched.</param>
        Task GetNextResource<T>(ContentFormatter<T> formatter, Action<T> callback, int offset = 1);
        
        /// <summary>
        /// Fetches the resource located delta resources before the current one
        /// from a specified source (implementation specific), formats it into the
        /// requested type using the given ContentFormatter and
        /// returns it to the callback function.
        /// </summary>
        /// <typeparam name="T">Type of the resource to create with the fetched data.</typeparam>
        /// <param name="formatter">Function to use to convert the raw data into a resource of type T.</param>
        /// <param name="callback">Function to which the created resource should be sent.</param>
        /// <param name="offset">Distance to the previous resource to be fetched.</param>
        Task GetPreviousResource<T>(ContentFormatter<T> formatter, Action<T> callback, int offset = 1);
#endif
        void MoveForward(int count = 1);

        void MoveBack(int count = 1);
    }
}
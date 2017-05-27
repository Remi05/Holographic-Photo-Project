// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// Thread-safe Singleton implementation.
    /// </summary>
    public abstract class MultiThreadedSingleton<T> where T : new()
    {
        private static T instance;
        private static object syncRoot = new Object();

        /// <summary>
        /// Thread-safe Singleton instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                        }
                    }
                }

                return instance;
            }
        }
    }
}
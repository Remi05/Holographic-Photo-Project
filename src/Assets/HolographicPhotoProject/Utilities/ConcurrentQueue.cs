// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// Generic thread safe queue.
    /// </summary>
    public class ConcurrentQueue<T>
    {
        private Queue<T> queue;
        private Object queueLock;

        public ConcurrentQueue()
        {
            queue = new Queue<T>();
            queueLock = new Object();
        }

        /// <summary>
        /// Safely returns the number of elements in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                lock (queueLock)
                {
                    return queue.Count;
                }
            }
        }

        /// <summary>
        /// Safely enqueues an object in the queue.
        /// </summary>
        /// <param name="obj">The object to enqueue.</param>
        public void Enqueue(T obj)
        {
            lock (queueLock)
            {
                queue.Enqueue(obj);
            }
        }

        /// <summary>
        /// Tries to dequeue safely.
        /// </summary>
        /// <param name="obj">The dequeued object, or the default value of T if the queue was empty.</param>
        /// <returns>True if the queue contained at least an element.</returns>
        public bool TryDequeue(out T obj)
        {
            obj = default(T);

            lock (queueLock)
            {
                if (queue.Count > 0)
                {
                    obj = queue.Dequeue();
                    return true;
                }
            }

            return false;
        }
    }
}
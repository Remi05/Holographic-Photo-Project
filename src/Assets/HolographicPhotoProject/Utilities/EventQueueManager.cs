// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace HolographicPhotoProject.Utilities
{
    public class EventQueueManager : MonoBehaviour
    {
        private static object syncRoot = new object();

        private static ConcurrentQueue<Action> executionQueue = new ConcurrentQueue<Action>();

        public static EventQueueManager Instance { get; private set; }

        /// <summary>
        /// Initializes the instance of EventQueueManager (Must be called on the main thread).
        /// </summary>
        public static void Init()
        {
            if (Instance == null)
            {
                lock (syncRoot)
                {
                    if (Instance == null)
                    {
                        GameObject gameObject = new GameObject("EventQueueManager");
                        Instance = gameObject.AddComponent<EventQueueManager>();
                    }
                }
            }
        }

        /// <summary>
        /// Indicates if the execution queue is empty or not.
        /// </summary>
        public bool IsEmpty { get { return executionQueue.Count == 0; } }

        /// <summary>
        /// If actions exist in the execution queue, lock the thread and invoke all the actions in the queue.
        /// </summary>
        private void Update()
        {
            if (IsEmpty)
            {
                return;
            }

            Action action;

            while (executionQueue.TryDequeue(out action))
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Call one last update to clear the thread and destroy the manager object.
        /// </summary>
        private void OnDestroy()
        {
            Update();
            DestroyObject(gameObject);
            Instance = null;
        }

        /// <summary>
        /// Lock the thread and add the action to the end of the execution queue.
        /// </summary>
        public void Execute(Action action)
        {
            executionQueue.Enqueue(action);
        }
    }
}

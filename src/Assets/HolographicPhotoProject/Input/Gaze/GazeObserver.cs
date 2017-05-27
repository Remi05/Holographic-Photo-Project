// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HolographicPhotoProject.Input.Gaze
{
    /// <summary>
    /// Lets you know when the gaze is on the game object or one of its children.
    /// </summary>
    public class GazeObserver : MonoBehaviour
    {
        public event Action FocusEntered;
        public event Action FocusExited;

        /// <summary>
        /// Indicates if the object is being gazed at.
        /// </summary>
        public bool IsGazed { get { return IsChild(GazeManager.Instance.HitObject); } }

        private void Start()
        {
            GazeManager.Instance.FocusedObjectChanged += GazeManager_FocusedObjectChanged;
        }

        private void OnDestroy()
        {
            if (GazeManager.Instance != null)
            {
                GazeManager.Instance.FocusedObjectChanged -= GazeManager_FocusedObjectChanged;
            }
        }

        /// <summary>
        /// Triggers the FocusEntered and FocusExited events.
        /// </summary>
        /// <param name="previousObject">The object that was previously being gazed at.</param>
        /// <param name="newObject">The object that is currently being gazed at.</param>
        private void GazeManager_FocusedObjectChanged(GameObject previousObject, GameObject newObject)
        {
            if (!IsChild(newObject))
            {
                if (IsChild(previousObject))
                {
                    FocusExited.RaiseEvent();
                }
            }
            else if (!IsChild(previousObject))
            {
                FocusEntered.RaiseEvent();
            }
        }

        private bool IsChild(GameObject go)
        {
            return go != null && go.transform.IsChildOf(gameObject.transform);
        }
    }
}
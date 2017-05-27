// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Input.Gaze;
using UnityEngine;
using UnityEngine.UI;

namespace HolographicPhotoProject.UI.Tooltip
{
    /// <summary>
    /// This component will show its game object when its parent is being gazed at for a certain amount of time.
    /// </summary>
    public class Tooltip : MonoBehaviour
    {
        private static class Constants
        {
            public const float ShowDelayDefault = 1f;
            public const float HideDelayDefault = 0.5f;
        }

        [Tooltip("Delay before showing the tooltip.")]
        public float ShowDelay = Constants.ShowDelayDefault;

        [Tooltip("Delay before hiding the tooltip.")]
        public float HideDelay = Constants.HideDelayDefault;

        private GazeObserver parentsGazeObserver;

        protected void Start()
        {
            parentsGazeObserver = GetComponentInParent<GazeObserver>();

            if (parentsGazeObserver != null)
            {
                parentsGazeObserver.FocusEntered += ShowIfFocusedWithDelay;
                parentsGazeObserver.FocusExited += HideIfNotFocusedWithDelay;
            }
            else
            {
                Debug.LogError("The Tooltip.cs script expects its parent to have a GazeObserver.cs.");
            }

            ChangeVisibility(false);
        }

        private void OnDestroy()
        {
            parentsGazeObserver = GetComponentInParent<GazeObserver>();

            if (parentsGazeObserver != null)
            {
                parentsGazeObserver.FocusEntered -= ShowIfFocusedWithDelay;
                parentsGazeObserver.FocusExited -= HideIfNotFocusedWithDelay;
            }
        }

        /// <summary>
        /// Shows the tooltip after a delay of ShowDelay if the parent is focused.
        /// </summary>
        private void ShowIfFocusedWithDelay()
        {
            Invoke("ShowIfFocused", ShowDelay);
        }

        /// <summary>
        /// Hides the tooltip after a delay of HideDelay if the parent is not focused.
        /// </summary>
        private void HideIfNotFocusedWithDelay()
        {
            Invoke("HideIfNotFocused", HideDelay);
        }

        /// <summary>
        /// Shows the toolbar if the parent is focused.
        /// </summary>
        private void ShowIfFocused()
        {
            if (!parentsGazeObserver.IsGazed)
            {
                return;
            }

            ChangeVisibility(true);
        }

        /// <summary>
        /// Shows the toolbar if the parent is not focused.
        /// </summary>
        private void HideIfNotFocused()
        {
            if (parentsGazeObserver.IsGazed)
            {
                return;
            }

            ChangeVisibility(false);
        }

        /// <summary>
        /// Changes the visibility of the tooltip.
        /// </summary>
        private void ChangeVisibility(bool visibility)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = visibility;
            }

            foreach (var renderer in GetComponentsInChildren<Canvas>())
            {
                renderer.enabled = visibility;
            }
        }
    }
}

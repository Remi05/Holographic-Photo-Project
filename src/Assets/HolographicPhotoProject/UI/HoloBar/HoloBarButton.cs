// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HolographicPhotoProject.Input.Gaze;
using HolographicPhotoProject.Utilities;
using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.Holobar
{
    [RequireComponent(typeof(GazeObserver))]
    public class HoloBarButton : MonoBehaviour, IInputClickHandler
    {
        /// <summary>
        /// Triggered when the user clicks on the button.
        /// </summary>
        public event Action Clicked;

        [Tooltip("Material of the button in normal state.")]
        public Material Normal;

        [Tooltip("Material of the button in hovered state.")]
        public Material Hovered;

        private bool isHovered;
        private GazeObserver gazeObserver;

        private void Start()
        {
            gazeObserver = GetComponent<GazeObserver>();
        }

        private void Update()
        {
            if (gazeObserver.IsGazed && !isHovered)
            {
                MaterialsHelper.ChangeMaterial(transform, Hovered);
                isHovered = true;
            }
            else if (!gazeObserver.IsGazed && isHovered)
            {
                MaterialsHelper.ChangeMaterial(transform, Normal);
                isHovered = false;
            }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            Clicked.RaiseEvent();
        }
    }
}
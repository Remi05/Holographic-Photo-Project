// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.AdjustmentBox
{
    /// <summary>
    /// Component that binds the rotatable functionality to the rotator.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        private void Start()
        {
            var rotatableComponent = gameObject.GetComponent<Rotatable>();
            if (rotatableComponent == null)
            {
                Debug.LogError("Could not find the Rotatable component.");
                return;
            }

            var rotatorViewComponent = gameObject.GetComponentInChildren<RotatorView>(true);
            if (rotatorViewComponent == null)
            {
                Debug.LogError("Could not find a RotatorView component in any of the children objects.");
                return;
            }

            rotatableComponent.ShouldRotateWithPinch = false;

            rotatorViewComponent.Pressed += () =>
            {
                rotatableComponent.StartRotating(rotatorViewComponent.InputSource,
                                                 rotatorViewComponent.InputSourceId);
            };

            rotatorViewComponent.Clicked += rotatableComponent.StopRotating;
            rotatorViewComponent.Released += rotatableComponent.StopRotating;
        }
    }
}

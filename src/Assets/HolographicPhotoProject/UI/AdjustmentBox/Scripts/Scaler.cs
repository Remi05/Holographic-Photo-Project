// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.AdjustmentBox
{
    /// <summary>
    /// Component that binds the scaling functionality to the scaling view.
    /// </summary>
    public class Scaler : MonoBehaviour
    {
        private void Start()
        {
            var scalableComponent = gameObject.GetComponent<Scalable>();
            if (scalableComponent == null)
            {
                Debug.LogError("Could not find a Scalable component in any of the children objects.");
                return;
            }

            var scalerViewComponent = gameObject.GetComponentInChildren<ScalerView>(true);
            if (scalerViewComponent == null)
            {
                Debug.LogError("Could not find a ScalerView component in any of the children objects.");
                return;
            }

            scalableComponent.ShouldScaleWithPinch = false;

            scalerViewComponent.Pressed += () =>
            {
                scalableComponent.StartScaling(scalerViewComponent.InputSource,
                                               scalerViewComponent.InputSourceId);
            };

            scalerViewComponent.Clicked += scalableComponent.StopScaling;
            scalerViewComponent.Released += scalableComponent.StopScaling;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.UI.AdjustmentBox
{
    /// <summary>
    /// Component that binds the placeable functionality to the placer view.
    /// </summary>
    public class Placer : MonoBehaviour
    {
        private void Start()
        {
            var placeableComponent = gameObject.GetComponent<Placeable>();
            if (placeableComponent == null)
            {
                Debug.LogError("Could not find the Placeable component.");
                return;
            }

            var placerViewComponent = gameObject.GetComponentInChildren<PlacerView>(true);
            if (placerViewComponent == null)
            {
                Debug.LogError("Could not find an PlacerView component in any of the children objects.");
                return;
            }
 
            placerViewComponent.Pressed += () => 
            {
                if (placeableComponent.IsPlacing)
                {
                    placeableComponent.StopPlacing();
                }
                else
                {
                    placeableComponent.StartPlacing();
                }
            };
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// Makes the scale of the object constant even when the parent is scaled.
    /// </summary>
    public class FixedScale : MonoBehaviour
    {
        /// <summary>
        /// Whether the initial scale has already been saved or not.
        /// </summary>
        private bool isFixedScaleSet = false;

        private Vector3 localFixedScale;
        /// <summary>
        /// The local scale at which the object's transform is fixed.
        /// </summary>
        public Vector3 LocalFixedScale
        {
            get { return localFixedScale; }

            private set
            {
                localFixedScale = value;
                isFixedScaleSet = true;
            }
        }

        /// <summary>
        /// Initializes the component by saving the initial local scale of the object.
        /// </summary>
        private void Start()
        {
            if (!isFixedScaleSet)
            {
                SaveInitialScale();
            }
        }

        /// <summary>
        /// Updates the object by resetting the scale to it's initial value, keeping it fixed.
        /// </summary>
        private void Update()
        {
            // Dissociate the object from it's parent so that it's world scale is not affected by it's parent's.
            Transform parent = transform.parent;
            transform.parent = null;

            // Set the object's scale to the saved scale.
            transform.localScale = LocalFixedScale;

            // Reset the object's parent.
            transform.parent = parent;
        }

        public void SaveInitialScale()
        {
            // Save the initial world scale of the object.
            LocalFixedScale = transform.lossyScale;
        }
    }
}

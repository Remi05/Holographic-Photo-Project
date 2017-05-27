// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// Keeps the proportions of the object even when the parent is scaled, and 
    /// limits the minimum scale of the object to its initial value.
    /// </summary>
    public class FixedProportions : MonoBehaviour
    {
        /// <summary>
        /// Whether the initial scale has already been saved or not.
        /// </summary>
        private bool isFixedScaleSet = false;

        /// <summary>
        /// The previous parent transform used to know if the parent has changed.
        /// </summary>
        private Transform previousParent = null;

        /// <summary>
        /// The initial smallest component of the parent's scale.
        /// </summary>
        private float scaleFactor;

        private Vector3 fixedScale;
        /// <summary>
        /// The local scale at which the object's transform is fixed.
        /// </summary>
        public Vector3 FixedScale
        {
            get { return fixedScale; }

            private set
            {
                fixedScale = value;
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
            if (transform.parent == null)
            {
                return;
            }

            // Check if the parent has changed and update the scale factor accordingly.
            if (transform.parent != previousParent)
            {
                scaleFactor = MinComponent(transform.parent.lossyScale);
                previousParent = transform.parent;
            }

            // Dissociate the object from it's parent so that it's world scale is not affected by it's parent's.
            Transform parent = transform.parent;
            transform.parent = null;

            // Scales the object proportionately to the parent's smallest scale component. Limits the minimum size to the initial size.
            transform.localScale = Vector3.Max(FixedScale, FixedScale * MinComponent(parent.lossyScale) / scaleFactor);

            // Reset the object's parent.
            transform.parent = parent;
        }

        private float MinComponent(Vector3 vector)
        {
            return Mathf.Min(vector.x, vector.y, vector.z);
        }

        public void SaveInitialScale()
        {
            // Save the initial world scale of the object.
            FixedScale = transform.lossyScale;
        }
    }
}

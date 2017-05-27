// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity;
using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.AdjustmentBox
{
    /// <summary>
    /// Component that allows moving, scaling and rotating an object. It also
    /// has a rollback feature, to cancel changes.
    /// This one or more of the following components : Placer, Rotator or Scaler.
    /// </summary>
    public class Adjustable : MonoBehaviour
    {
        public event Action AdjustingStarted;
        public event Action AdjustingStopped;
        public event Action AdjustingCanceled;
        public event Action AdjustingConfirmed;

        /// <summary>
        /// The ID with which to save the anchor.
        /// </summary>
        public string AnchorId { get; set; }

        /// <summary>
        /// Indicates whether the target object is being adjusted or not.
        /// </summary>
        public bool IsAdjusting { get; private set; }

        [Tooltip("The material of the box handles.")]
        public Material AdjustmentBoxHandlesMaterial;

        [Tooltip("The material of the bounding box placer.")]
        public Material AdjustmentBoxPlacerMaterial;

        [Tooltip("The material of the bounding box when it hits maximum or minimum size.")]
        public Material AdjustmentBoxMinMaxMaterial;

        private WorldAnchorManager anchorManager;

        // Previous transform
        private Vector3 prevPosition;
        private Vector3 prevScale;
        private Quaternion prevLocalRotation;
        private Quaternion prevWorldRotation;

        /// <summary>
        /// The game object targeted by the adjustable script.
        /// </summary>
        public GameObject TargetGameObject { get; set; }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        private void Start()
        {
            // Temporary
            if (TargetGameObject == null)
            {
                TargetGameObject = gameObject;
            }

            if (AnchorId == null)
            {
                AnchorId = Guid.NewGuid().ToString();
            }

            anchorManager = WorldAnchorManager.Instance;
            if (anchorManager == null)
            {
                Debug.LogError("This script expects that you have a WorldAnchorManager component in your scene.");
            }

            var placeableComponent = gameObject.GetComponentInChildren<Placeable>(true);
            if (placeableComponent != null)
            {
                placeableComponent.TargetGameObject = TargetGameObject;
            }

            var rotatableComponent = gameObject.GetComponentInChildren<Rotatable>(true);
            if (rotatableComponent != null)
            {
                rotatableComponent.TargetGameObject = TargetGameObject;
            }

            var scalableComponent = gameObject.GetComponentInChildren<Scalable>(true);
            if (scalableComponent != null)
            {
                scalableComponent.TargetGameObject = TargetGameObject;
                scalableComponent.ScalingLimitZoneEntered += AdjustmentBoxSetWarningColor;
                scalableComponent.ScalingLimitZoneExited += AdjustmentBoxSetDefaultColor;
            }
        }

        /// <summary>
        /// Handler for the OnDisable event which cancels the changes applied to the target object.
        /// </summary>
        private void OnDisable()
        {
            CancelChanges();
        }

        /// <summary>
        /// Handler for the OnDestroy event which cancels the changes applied to the target object.
        /// </summary>
        private void OnDestroy()
        {
            CancelChanges();
        }

        /// <summary>
        /// Stops adjusting the target object and reverts the changes applied to it.
        /// </summary>
        public void CancelChanges()
        {
            if (!IsAdjusting)
            {
                return;
            }

            StopAdjusting();
            RevertTargetChanges();

            if (anchorManager != null)
            {
                Debug.Log(string.Format("Attempting to attach anchor for GameObject {0} with ID {1}", TargetGameObject.name, AnchorId));
                anchorManager.AttachAnchor(TargetGameObject, AnchorId);
            }

            AdjustingCanceled.RaiseEvent();
        }

        /// <summary>
        /// Stops adjusting the target object and saves the world anchor if possible.
        /// </summary>
        public void ConfirmChanges()
        {
            if (!IsAdjusting)
            {
                return;
            }

            StopAdjusting();

            if (anchorManager != null)
            {
                Debug.Log(string.Format("Attempting to attach anchor for GameObject {0} with ID {1}", TargetGameObject.name, AnchorId));
                anchorManager.AttachAnchor(TargetGameObject, AnchorId);
            }

            AdjustingConfirmed.RaiseEvent();
        }

        /// <summary>
        /// Disables all existing children adjustment components.
        /// </summary>
        private void DisableAdjustingComponents()
        {
            var placeableComponent = gameObject.GetComponentInChildren<Placeable>(true);
            if (placeableComponent != null)
            {
                placeableComponent.enabled = false;
            }

            var rotatableComponent = gameObject.GetComponentInChildren<Rotatable>(true);
            if (rotatableComponent != null)
            {
                rotatableComponent.enabled = false;
            }

            var scalableComponent = gameObject.GetComponentInChildren<Scalable>(true);
            if (scalableComponent != null)
            {
                scalableComponent.enabled = false;
            }
        }

        /// <summary>
        /// Enables all existing children adjustment components.
        /// </summary>
        private void EnableAdjustingComponents()
        {
            var placeableComponent = gameObject.GetComponentInChildren<Placeable>(true);
            if (placeableComponent != null)
            {
                placeableComponent.enabled = true;
            }

            var rotatableComponent = gameObject.GetComponentInChildren<Rotatable>(true);
            if (rotatableComponent != null)
            {
                rotatableComponent.enabled = true;
            }

            var scalableComponent = gameObject.GetComponentInChildren<Scalable>(true);
            if (scalableComponent != null)
            {
                scalableComponent.enabled = true;
            }
        }

        /// <summary>
        /// Reverts the changes applied to the target object's transform.
        /// </summary>
        private void RevertTargetChanges()
        {
            TargetGameObject.transform.localRotation = prevLocalRotation;
            TargetGameObject.transform.localScale = prevScale;
            TargetGameObject.transform.position = prevPosition;
            TargetGameObject.transform.rotation = prevWorldRotation;
        }

        /// <summary>
        /// Saves the target GameObject's current transform.
        /// </summary>
        private void SaveTargetTransform()
        {
            prevLocalRotation = TargetGameObject.transform.localRotation;
            prevPosition = TargetGameObject.transform.position;
            prevScale = TargetGameObject.transform.localScale;
            prevWorldRotation = TargetGameObject.transform.rotation;
        }

        /// <summary>
        /// Starts adjusting the target object by enabling the adjustment components.
        /// </summary>
        public void StartAdjusting()
        {
            if (IsAdjusting)
            {
                return;
            }

            SaveTargetTransform();

            if (anchorManager != null && anchorManager.AnchorStore != null)
            {
                Debug.Log(string.Format("Attempting to remove anchor for GameObject : {0}", TargetGameObject.name));
                anchorManager.RemoveAnchor(TargetGameObject);
            }

            EnableAdjustingComponents();
            IsAdjusting = true;
            AdjustingStarted.RaiseEvent();
        }

        /// <summary>
        /// Stops adjusting the target object.
        /// </summary>
        private void StopAdjusting()
        {
            if (!IsAdjusting)
            {
                return;
            }

            DisableAdjustingComponents();
            IsAdjusting = false;
            AdjustingStopped.RaiseEvent();
        }

        /// <summary>
        /// Changes the adjustment box color to a warning color when the limit is reached.
        /// </summary>
        public void AdjustmentBoxSetWarningColor()
        {
            var placer = transform.FindChild("Placer");
            MaterialsHelper.ChangeMaterial(placer, AdjustmentBoxMinMaxMaterial);

            var rotator = transform.FindChild("Rotator");
            MaterialsHelper.ChangeMaterial(rotator, AdjustmentBoxMinMaxMaterial);

            var scaler = transform.FindChild("Scaler");
            MaterialsHelper.ChangeMaterial(scaler, AdjustmentBoxMinMaxMaterial);
        }

        /// <summary>
        /// Changes the adjustment box color to default.
        /// </summary>
        public void AdjustmentBoxSetDefaultColor()
        {
            var placer = transform.FindChild("Placer");
            MaterialsHelper.ChangeMaterial(placer, AdjustmentBoxPlacerMaterial);

            var rotator = transform.FindChild("Rotator");
            MaterialsHelper.ChangeMaterial(rotator, AdjustmentBoxHandlesMaterial);

            var scaler = transform.FindChild("Scaler");
            MaterialsHelper.ChangeMaterial(scaler, AdjustmentBoxHandlesMaterial);
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.AdjustmentBox
{
    /// <summary> 
    /// Component that allows rotating an object with your hand on HoloLens. 
    /// </summary> 
    public class Rotatable : MonoBehaviour, IInputHandler, ISourceStateHandler
    {
        private static class Constants
        {
            public const float DefaultRotationalSensitivity = 10f;
        }

        /// <summary>
        /// Triggered when the object starts being rotated.
        /// </summary>
        public event Action StartedRotating;

        /// <summary>
        /// Triggered when the object starts being rotated.
        /// </summary>
        public event Action StoppedRotating;

        [Tooltip("Should rotating start by pinching and end by releasing?")]
        public bool ShouldRotateWithPinch = false;

        [Tooltip("The GameObject to rotate.")]
        public GameObject TargetGameObject;

        /// <summary>
        /// Indicates whether the object is being rotated or not.
        /// </summary>
        public bool IsRotating { get; private set; }

        private IInputSource currentInputSource;
        private uint currentInputSourceId;
        private Vector3 prevHandPosition;
        private float rotationalSensitivity = Constants.DefaultRotationalSensitivity;

        private void Update()
        {
            if (IsRotating)
            {
                UpdateRotating();
            }           
        }

        private void OnDestroy()
        {
            StopRotating();
        }

        private void OnDisable()
        {
            StopRotating();
        }

        /// <summary>
        /// Handler called when the input source is pressed down which starts rotating.
        /// </summary>
        public void OnInputDown(InputEventData eventData)
        {
            if (ShouldRotateWithPinch)
            {
                StartRotating(eventData.InputSource, eventData.SourceId);
            }
        }

        /// <summary>
        /// Handler called when the input source is released which stops rotating.
        /// </summary>
        public void OnInputUp(InputEventData eventData)
        {
            if (currentInputSource != null 
                && eventData.SourceId == currentInputSourceId)
            {
                StopRotating();
            }
        }

        /// <summary>
        /// Hanlder called when the input source is detected.
        /// </summary>
        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // Part of ISourceStateHandler but we don't need it.
        }

        /// <summary>
        /// Hanlder called when the input source is lost which stops rotating.
        /// </summary>
        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (currentInputSource != null
                && eventData.SourceId == currentInputSourceId)
            {
                StopRotating();
            }
        }

        /// <summary>
        /// Starts rotating the object with the given input source.
        /// </summary>
        public void StartRotating(IInputSource inputSource, uint inputSourceId)
        {
            if (IsRotating)
            {
                return;
            }

            if (inputSource == null || !inputSource.SupportsInputInfo(inputSourceId, SupportedInputInfo.Position))
            {
                Debug.Log("The input source must provide positional data for Rotatable to be usable.");
                return;
            }

            currentInputSource = inputSource;
            currentInputSourceId = inputSourceId;

            currentInputSource.TryGetPosition(currentInputSourceId, out prevHandPosition);

            // Add self as a modal input handler, to get all inputs during the manipulation.
            InputManager.Instance.PushModalInputHandler(gameObject);

            IsRotating = true;
            StartedRotating.RaiseEvent();
        }

        /// <summary>
        /// Stops rotating the object.
        /// </summary>
        public void StopRotating()
        {
            if (!IsRotating)
            {
                return;
            }

            // Remove self as a modal input handler.
            InputManager.Instance.PopModalInputHandler();

            IsRotating = false;
            StoppedRotating.RaiseEvent();
        }

        /// <summary>
        /// Updates the object's rotation according to the hand's X 
        /// position scaled by the rotational sensitivity.
        /// </summary>
        private void UpdateRotating()
        {
            Vector3 handPosition;
            bool isValidPos = currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);
            if (!isValidPos)
            {
                return;
            }

            Vector3 prevLocalPos = TargetGameObject.transform.worldToLocalMatrix.MultiplyPoint(prevHandPosition);
            Vector3 curLocalPos = TargetGameObject.transform.worldToLocalMatrix.MultiplyPoint(handPosition);

            float lastAngle = Mathf.Atan2(prevLocalPos.z, prevLocalPos.x);
            float curAngle = Mathf.Atan2(curLocalPos.z, curLocalPos.x);
            float rotation = (curAngle - lastAngle) * Mathf.Rad2Deg;

            TargetGameObject.transform.Rotate(Vector3.up, -rotation * rotationalSensitivity);

            prevHandPosition = handPosition;
        }
    }
}

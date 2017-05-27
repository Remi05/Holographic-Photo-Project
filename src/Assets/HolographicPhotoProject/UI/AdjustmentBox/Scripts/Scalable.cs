// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.AdjustmentBox
{
    /// <summary> 
    /// Component that allows scaling an object with your hand on HoloLens. 
    /// It also has a minimal and maximal scale property to prevent the object from becoming
    /// too small or too big.
    /// </summary> 
    public class Scalable : MonoBehaviour, IInputHandler, ISourceStateHandler
    {
        private static class Constants
        {
            public const float DefaultScalingFactor = 5f;
            public const float DefaultMinimumScale = 0.5f;
            public const float DefaultMaximumScale = 3.0f;
            public const float SizeLimitMargin = 0.05f;
        }

        /// <summary>
        /// Triggered when the object starts being scaled.
        /// </summary>
        public event Action StartedScaling;

        /// <summary>
        /// Triggered when the object stops being scaled.
        /// </summary>
        public event Action StoppedScaling;

        /// <summary>
        /// Triggered when the object is at max or min size.
        /// </summary>
        public event Action ScalingLimitZoneEntered;

        /// <summary>
        /// Triggered when the object size is not at max or min.
        /// </summary>
        public event Action ScalingLimitZoneExited;

        [Tooltip("Should scaling start by pinching and end by releasing?")]
        public bool ShouldScaleWithPinch = false;

        [Tooltip("The maximum size of each object side.")]
        public float MaximumScale = Constants.DefaultMaximumScale;

        [Tooltip("The minimum size of each object side.")]
        public float MinimumScale = Constants.DefaultMinimumScale;

        [Tooltip("The GameObject to rotate.")]
        public GameObject TargetGameObject;

        private Vector3 scaleUpVector;
        private Vector3 grabPoint;
        private bool isScaling;
        private IInputSource currentInputSource;
        private uint currentInputSourceId;
        private Vector3 prevHandPosition;

        private bool IsInScalingLimitZone
        {
            get
            {
                var scaleDimension = TargetGameObject.transform.localScale.x;
                return (scaleDimension < (MinimumScale + Constants.SizeLimitMargin)
                     || scaleDimension > (MaximumScale - Constants.SizeLimitMargin));
            }
        }
        
        private void Update()
        {
            if (isScaling)
            {
                UpdateScaling();
            }

            if (isScaling && IsInScalingLimitZone)
            {
                ScalingLimitZoneEntered.RaiseEvent();
            }
            else
            {
                ScalingLimitZoneExited.RaiseEvent();
            }
        }

        /// <summary>
        /// Handler called when the object is about to be destroyed which stops scaling.
        /// </summary>
        private void OnDestroy()
        {
            StopScaling();
        }

        /// <summary>
        /// Handler called when the object is about to be disabled which stops scaling.
        /// </summary>
        private void OnDisable()
        {
            StopScaling();
        }

        /// <summary>
        /// Handler called when the input source is pressed down which starts scaling.
        /// </summary>
        public void OnInputDown(InputEventData eventData)
        {
            if (ShouldScaleWithPinch)
            {
                StartScaling(eventData.InputSource, eventData.SourceId);
            }
        }

        /// <summary>
        /// Handler called when the input source is released which stops scaling.
        /// </summary>
        public void OnInputUp(InputEventData eventData)
        {
            if (currentInputSource != null
                && eventData.SourceId == currentInputSourceId)
            {
                StopScaling();
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
                StopScaling();
            }
        }

        /// <summary>
        /// Starts scaling the object with the given input source.
        /// </summary>
        public void StartScaling(IInputSource inputSource, uint inputSourceId)
        {
            if (isScaling)
            {
                return;
            }

            if (inputSource == null || !inputSource.SupportsInputInfo(inputSourceId, SupportedInputInfo.Position))
            {
                Debug.Log("The provided input source does not support the required positional info.");
                return;
            }

            currentInputSource = inputSource;
            currentInputSourceId = inputSourceId;

            // Add self as a modal input handler, to get all inputs during the manipulation.
            InputManager.Instance.PushModalInputHandler(gameObject);

            currentInputSource.TryGetPosition(currentInputSourceId, out prevHandPosition);
            grabPoint = prevHandPosition;
            scaleUpVector = Vector3.Normalize(grabPoint - TargetGameObject.transform.position);

            isScaling = true;
            StartedScaling.RaiseEvent();
        }

        /// <summary>
        /// Stops scaling the object.
        /// </summary>
        public void StopScaling()
        {
            if (!isScaling)
            {
                return;
            }

            // TODO : Make sure it pops the right input handler.
            // Remove self as a modal input handler.
            InputManager.Instance.PopModalInputHandler();

            isScaling = false;
            StoppedScaling.RaiseEvent();
        }

        /// <summary>
        /// Scales the object according to the hand position and the current scale up vector.
        /// </summary>
        private void UpdateScaling()
        {
            Vector3 handPosition;
            bool isValidPosition = currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);
            if (!isValidPosition)
            {
                return;
            }

            var handDelta = handPosition - prevHandPosition;
            prevHandPosition = handPosition;
            float scaleDelta = Vector3.Dot(handDelta, scaleUpVector) * Constants.DefaultScalingFactor;

            var newScale = TargetGameObject.transform.localScale + new Vector3(scaleDelta, scaleDelta, scaleDelta);
            var minScale = new Vector3(MinimumScale, MinimumScale, MinimumScale);
            var maxScale = new Vector3(MaximumScale, MaximumScale, MaximumScale);

            TargetGameObject.transform.localScale = Vector3.Min(Vector3.Max(minScale, newScale), maxScale);
        }
    }
}

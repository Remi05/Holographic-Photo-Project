// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Based on the original HandDraggable.cs script from the Windows Holograhpics HoloToolkit.
// Src : https://github.com/Microsoft/HoloToolkit-Unity/blob/master/Assets/HoloToolkit/Input/Scripts/Interactions/HandDraggable.cs

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.AdjustmentBox
{
    /// <summary>
    /// Component that allows placing an object in the environment with the gaze.
    /// </summary>
    public class Placeable : MonoBehaviour, IInputHandler, IInputClickHandler, ISourceStateHandler
    {
        private static class Constants
        {
            public const float DefaultMaxRayCastDistance = 20.0f;
            public const float DefaultPlacingDistance = 2.0f;
            public const float HandPivotForwardOffset = -0.2f;
            public static readonly Vector3 HandPivotPositionOffset = new Vector3(0.0f, -0.2f, 0.0f);
        }

        // Events raised when the associated actions are completed.
        public event Action StartedPlacing;
        public event Action StoppedPlacing;
        public event Action StartedDragging;
        public event Action StartedPlacingWithGaze;
        public event Action StoppedDragging;
        public event Action StoppedPlacingWithGaze;

        private static GameObject currentlyPlacedObject = null;

        /// <summary>
        /// Indicates whether any object with the Placeable component is being placed at the moment.
        /// </summary>
        public static bool ExistsObjectInPlacingMode
        {
             get { return currentlyPlacedObject != null; }
        }

        [Tooltip("Maximum distance at which the raycast will check for collisions with the environment.")]
        public float MaxRaycastDistance = Constants.DefaultMaxRayCastDistance;

        [Tooltip("Should the object be picked up when you click on it?")]
        public bool PickUpOnClick = false;

        [Tooltip("The GameObject to rotate.")]
        public GameObject TargetGameObject;

        /// <summary>
        /// Indicates whether the target object is being placed or not.
        /// </summary>
        public bool IsPlacing { get; private set; }

        private SpatialMappingManager spatialMappingManager;
        private IInputSource currentInputSource;
        private uint currentInputSourceId;

        private float placingDistance = Constants.DefaultPlacingDistance;

        private Quaternion gazeAngularOffset;
        private bool isDragging;
        private bool isInitialized;
        private bool isPlacingWithGaze;
        private bool shouldPickUp;

        private void Start()
        {
            spatialMappingManager = SpatialMappingManager.Instance;
            if (spatialMappingManager == null)
            {
                Debug.LogError("This script expects that you have a SpatialMappingManager component in your scene.");
            }

            isInitialized = true;
        }

        private void Update()
        {
            if (shouldPickUp)
            {
                StartPlacing();
                shouldPickUp = false;
            }

            if (IsPlacing)
            {
                UpdatePlacing();
            }
        }

        private void OnDestroy()
        {
            StopPlacing();
        }

        private void OnDisable()
        {
            StopPlacing();
        }

        // From HandDraggable.cs
        /// <summary>
        /// Gets the pivot position for the hand, which is approximated to the base of the neck.
        /// </summary>
        /// <returns>Pivot position for the hand.</returns>
        private Vector3 GetHandPivotPosition()
        {
            return Camera.main.transform.position + Constants.HandPivotPositionOffset + Camera.main.transform.forward * Constants.HandPivotForwardOffset;
        }

        /// <summary>
        /// Handler called when the object is clicked.
        /// </summary>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (PickUpOnClick)
            {
                StartPlacing();
            }
        }

        /// <summary>
        /// Handler called when the input source is pressed down which
        /// switches from placing with the gaze to placing with hand gestures.
        /// </summary>
        public void OnInputDown(InputEventData eventData)
        {
            if (IsPlacing && isPlacingWithGaze)
            {
                if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
                {
                    Debug.LogError("The input source must provide positional data for Placeable to be usable.");
                    return;
                }

                currentInputSource = eventData.InputSource;
                currentInputSourceId = eventData.SourceId;

                // This is commented out because StartDragging is not working properly. Instead, we stop placing.
                //// StartDragging();

                StopPlacing();
            }
        }

        /// <summary>
        /// Handler called when the input source is released which
        /// stops placing if currently placing with hand gestures.
        /// </summary>
        public void OnInputUp(InputEventData eventData)
        {
            if (isDragging && currentInputSource != null
                && eventData.SourceId == currentInputSourceId)
            {
                StopPlacing();
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
        /// Hanlder called when the input source is lost.
        /// </summary>
        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (isDragging && currentInputSource != null
                && eventData.SourceId == currentInputSourceId)
            {
                StopPlacing();
            }
        }

        /// <summary>
        /// Places the object onto the environment mesh.
        /// </summary>
        private void Place(Vector3 position, Quaternion rotation)
        {
            // Temporary : Makes sure the cursor is above the HoloBar.
            position.y -= 0.01f;

            TargetGameObject.transform.position = position;
            TargetGameObject.transform.rotation = rotation;
        }

        /// <summary>
        /// Starts dragging the object.
        /// </summary>
        private void StartDragging()
        {
            if (isDragging)
            {
                return;
            }

            StopPlacingWithGaze();

            isDragging = true;

            Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;

            Vector3 handPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

            Vector3 pivotPosition = GetHandPivotPosition();

            Vector3 objDirection = Vector3.Normalize(gazeHitPosition - pivotPosition);
            Vector3 handDirection = Vector3.Normalize(handPosition - pivotPosition);
            objDirection = Camera.main.transform.InverseTransformDirection(objDirection);
            handDirection = Camera.main.transform.InverseTransformDirection(handDirection);

            // Store the initial offset between the hand and the object, so that we can consider it when dragging.
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// Puts the object in a state where it can be placed.
        /// </summary>
        public void StartPlacing(bool computeObjectDistance = true)
        {
            if (IsPlacing)
            {
                return;
            }

            if (!isInitialized)
            {
                // If the component is not initialized yet, indicate that PickUp() should be called on the next update.
                shouldPickUp = true;
                return;
            }

            InputManager.Instance.PushModalInputHandler(gameObject);

            // By default, the object should remain at the same distance from
            // the user when picked up but not if it placed for the first time.
            if (computeObjectDistance)
            {
                placingDistance = (TargetGameObject.transform.position - Camera.main.transform.position).magnitude;
            }

            currentlyPlacedObject = TargetGameObject;

            IsPlacing = true;
            StartPlacingWithGaze();
            StartedPlacing.RaiseEvent();
        }

        /// <summary>
        /// Starts placing the object using the gaze.
        /// </summary>
        private void StartPlacingWithGaze()
        {
            if (isPlacingWithGaze)
            {
                return;
            }

            StopDragging();

            isPlacingWithGaze = true;
            StartedPlacingWithGaze.RaiseEvent();
        }

        // From HandDraggable.cs
        /// <summary>
        /// Stops dragging the object.
        /// </summary>
        private void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
            currentInputSource = null;
            StoppedDragging.RaiseEvent();
        }

        /// <summary>
        /// Locks the object to the environment.
        /// </summary>
        public void StopPlacing()
        {
            if (!IsPlacing)
            {
                return;
            }

            StopDragging();
            StopPlacingWithGaze();

            currentlyPlacedObject = null;

            IsPlacing = false;
            StoppedPlacing.RaiseEvent();

            InputManager.Instance.PopModalInputHandler();
        }

        /// <summary>
        /// Stops placing the object with the gaze.
        /// </summary>
        private void StopPlacingWithGaze()
        {
            if (!isPlacingWithGaze)
            {
                return;
            }

            isPlacingWithGaze = false;
            StoppedPlacingWithGaze.RaiseEvent();
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            Vector3 newHandPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition);

            Vector3 pivotPosition = GetHandPivotPosition();

            Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);
            newHandDirection = Camera.main.transform.InverseTransformDirection(newHandDirection);

            Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
            targetDirection = Camera.main.transform.TransformDirection(targetDirection);

            Vector3 headPosition = Camera.main.transform.position;

            UpdatePosition(headPosition, targetDirection);
        }

        /// <summary>
        /// Update the position of the object being placed.
        /// </summary>
        private void UpdatePlacing()
        {
            if (isDragging)
            {
                UpdateDragging();
            }
            else if (isPlacingWithGaze)
            {
                UpdatePlacingWithGaze();
            }
        }

        /// <summary>
        /// Update the position of the object being placed with the gaze.
        /// </summary>
        private void UpdatePlacingWithGaze()
        {
            Vector3 headPosition = Camera.main.transform.position;
            Vector3 gazeDirection = Camera.main.transform.forward;
            UpdatePosition(headPosition, gazeDirection);
        }

        /// <summary>
        /// Project the object onto the environment mesh.
        /// </summary>
        private void UpdatePosition(Vector3 position, Vector3 direction)
        {
            // Rotate this object to face the user.
            Quaternion rotation = Camera.main.transform.localRotation;
            rotation.x = 0;
            rotation.z = 0;

            // Do a raycast into the world that will only hit the Spatial Mapping mesh.
            RaycastHit hitInfo;
            if (Physics.Raycast(position, direction, out hitInfo, MaxRaycastDistance, spatialMappingManager.LayerMask))
            {
                Place(hitInfo.point, rotation);
            }
            else
            {
                // If no collision is detected within the specified distance,
                // the object is moved at a reasonable distance of the camera.
                Vector3 newPosition = Camera.main.transform.position + Camera.main.transform.forward * placingDistance;
                Place(newPosition, rotation);
            }
        }
    }
}

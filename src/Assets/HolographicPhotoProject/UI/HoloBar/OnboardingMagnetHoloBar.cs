// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Input.VoiceCommands;
using HolographicPhotoProject.UI.Holobar;
using HoloToolkit.Unity;
using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.HoloBar
{
    public class OnboardingMagnetHoloBar : HoloBar
    {
        private static class Constants
        {
            public const string ConfirmationBar = "AdjustConfirmationBar";
            public const string SouvenirBar = "SouvenirBar";

            public const string AdjustButton = "Adjust";
            public const string CancelButton = "Cancel";
            public const string ConfirmButton = "Confirm";

            public const float SideOffset = -1.0f;
            public const float SideRotation = -90.0f;
            public const float SidePositionDeadZoneSize = 5.0f;

            public const float HideHoloBarDelay = 20f;
            public const float ShowHoloBarDelay = 0;
        }

        public event Action AdjustClicked;
        public event Action CancelClicked;
        public event Action ConfirmClicked;

        private HoloBarButton adjustButton;
        private HoloBarButton cancelButton;
        private HoloBarButton confirmButton;

        protected override void Start()
        {
            base.Start();

            var souvenirBar = transform.FindChild(Constants.SouvenirBar);
            var confirmationBar = transform.FindChild(Constants.ConfirmationBar);

            // Gets the holobar buttons
            adjustButton = souvenirBar.FindChild(Constants.AdjustButton).gameObject.GetComponent<HoloBarButton>();
            cancelButton = confirmationBar.FindChild(Constants.CancelButton).gameObject.GetComponent<HoloBarButton>();
            confirmButton = confirmationBar.FindChild(Constants.ConfirmButton).gameObject.GetComponent<HoloBarButton>();

            // Attaches the clicked events to the buttons
            adjustButton.Clicked += () => { RaiseIfNotInConfirmationState(AdjustClicked); };
            cancelButton.Clicked += () => { CancelClicked.RaiseEvent(); };
            confirmButton.Clicked += () => { ConfirmClicked.RaiseEvent(); };

            // Sets up the voice commands
            keywords.Add(VoiceCommandsManager.AdjustKeyword, () => { RaiseIfNotInConfirmationState(AdjustClicked); });
            keywords.Add(VoiceCommandsManager.CancelKeyword, () => { CancelClicked.RaiseEvent(); });
            keywords.Add(VoiceCommandsManager.ConfirmKeyword, () => { ConfirmClicked.RaiseEvent(); });

            // Attaches the holobar flow methods to the buttons
            AdjustClicked += () => { ChangeBar(Constants.ConfirmationBar); };
            CancelClicked += () => { ChangeBar(Constants.SouvenirBar); };
            ConfirmClicked += () => { ChangeBar(Constants.SouvenirBar); };
        }

        private void Update()
        {
            PlaceHoloBar();
        }

        /// <summary>
        /// Places the HoloBar on the appropriate side of the parent object.
        /// </summary>
        private void PlaceHoloBar()
        {
            // We temporarily move the object to the local center. This is necessary because we want the local position
            // of the camera to be relative to the center. Removing this makes the holobar switch repeatedly between
            // the sides of the parent object. 
            var souvenirLocalPosition = transform.localPosition;
            transform.localPosition = Vector3.zero;
            Vector3 localCameraPos = gameObject.transform.InverseTransformPoint(Camera.main.transform.position);
            transform.localPosition = souvenirLocalPosition;

            float yAngle = Mathf.Atan2(localCameraPos.z, localCameraPos.x) * Mathf.Rad2Deg;

            // Wraps yAngle in the first quadrant, and then check if this angle is outside the deadzone.
            // This prevents the holobar from ping-ponging between two sides.
            if (transform.parent != null
                && Math.Abs((yAngle % 90) - 45) > Constants.SidePositionDeadZoneSize)
            {
                var parentTransform = transform.parent;
                float side = Mathf.Round(yAngle / 90.0f) + Constants.SideOffset;
                gameObject.transform.RotateAround(parentTransform.position, Vector3.up, side * Constants.SideRotation);
            }
        }

        /// <summary>
        /// Triggers the AdjustClicked event (effectively simulating a click on the "Adjust" button).
        /// </summary>
        public void StartAdjusting()
        {
            ChangeBar(Constants.ConfirmationBar);
            AdjustClicked.RaiseEvent();
        }

        /// <summary>
        /// When the parent gets the focus, brings up the holobar if it was hidden.
        /// </summary>
        protected override void ShowHoloBar_OnParentFocusEnter()
        {
            if (currentStateName == null)
            {
                ChangeBarWithDelay(Constants.SouvenirBar, Constants.ShowHoloBarDelay);
            }
            else
            {
                // This is in case we lost the focus and get it back before the bar disappears, we want go cancel hiding the bar.
                nextStateName = currentStateName;
            }
        }

        /// <summary>
        /// When the parent loses the focus, hides the holobar after a delay defined by Constants.HideHoloBarDelay.
        /// </summary>
        protected override void HideHoloBar_OnParentFocusExit()
        {
            if (!IsInConfirmationState())
            {
                ChangeBarWithDelay(null, Constants.HideHoloBarDelay);
            }
        }

        /// <summary>
        /// Indicates if the holobar is currently in confirmation state.
        /// </summary>
        private bool IsInConfirmationState()
        {
            return currentStateName == Constants.ConfirmationBar;
        }

        /// <summary>
        /// Raises the given event if the holobar is not in confirmation state.
        /// </summary>
        private void RaiseIfNotInConfirmationState(Action a)
        {
            if (!IsInConfirmationState())
            {
                a.RaiseEvent();
            }
        }
    }
}
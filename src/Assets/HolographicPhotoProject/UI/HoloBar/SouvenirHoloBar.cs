// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Input.VoiceCommands;
using HolographicPhotoProject.UI.Holobar;
using HoloToolkit.Unity;
using System;
using UnityEngine;

namespace HolographicPhotoProject.UI.HoloBar
{
    public class SouvenirHoloBar : HoloBar
    {
        private static class Constants
        {
            public const string AdjustConfirmationBar = "AdjustConfirmationBar";
            public const string RemoveConfirmationBar = "RemoveConfirmationBar";
            public const string SouvenirBar = "SouvenirBar";
            public const string HiddenBar = "HiddenBar";

            public const string AdjustButton = "Adjust";
            public const string CancelButton = "Cancel";
            public const string ConfirmButton = "Confirm";
            public const string RemoveButton = "Remove";
            public const string HideHoloBarButton = "Hide";
            public const string ShowHoloBarButton = "ShowBar";

            public const float SideOffset = -1.0f;
            public const float SideRotation = -90.0f;
            public const float SidePositionDeadZoneSize = 5.0f;

            public const float HideHoloBarDelay = 20f;
            public const float ShowHoloBarDelay = 0;
        }

        public event Action AdjustClicked;
        public event Action AdjustCancelClicked;
        public event Action AdjustConfirmClicked;
        public event Action RemoveCancelClicked;
        public event Action RemoveConfirmClicked;
        public event Action RemoveClicked;
        public event Action HideHoloBarClicked;
        public event Action ShowHoloBarClicked;

        private HoloBarButton adjustButton;
        private HoloBarButton adjustCancelButton;
        private HoloBarButton adjustConfirmButton;
        private HoloBarButton hideHoloBarButton;
        private HoloBarButton removeButton;
        private HoloBarButton removeCancelButton;
        private HoloBarButton removeConfirmButton;
        private HoloBarButton showHoloBarButton;

        protected override void Start()
        {
            base.Start();

            var souvenirBar = transform.FindChild(Constants.SouvenirBar);
            var adjustConfirmationBar = transform.FindChild(Constants.AdjustConfirmationBar);
            var removeConfirmationBar = transform.FindChild(Constants.RemoveConfirmationBar);
            var hiddenBar = transform.FindChild(Constants.HiddenBar);

            // Gets the holobar buttons
            adjustButton = souvenirBar.FindChild(Constants.AdjustButton).gameObject.GetComponent<HoloBarButton>();
            adjustCancelButton = adjustConfirmationBar.FindChild(Constants.CancelButton).gameObject.GetComponent<HoloBarButton>();
            adjustConfirmButton = adjustConfirmationBar.FindChild(Constants.ConfirmButton).gameObject.GetComponent<HoloBarButton>();
            removeButton = souvenirBar.FindChild(Constants.RemoveButton).gameObject.GetComponent<HoloBarButton>();
            removeCancelButton = removeConfirmationBar.FindChild(Constants.CancelButton).gameObject.GetComponent<HoloBarButton>();
            removeConfirmButton = removeConfirmationBar.FindChild(Constants.ConfirmButton).gameObject.GetComponent<HoloBarButton>();
            hideHoloBarButton = souvenirBar.FindChild(Constants.HideHoloBarButton).gameObject.GetComponent<HoloBarButton>();
            showHoloBarButton = hiddenBar.FindChild(Constants.ShowHoloBarButton).gameObject.GetComponent<HoloBarButton>();

            // Attaches the clicked events to the buttons
            showHoloBarButton.Clicked += () => { RaiseIfNotInConfirmationState(ShowHoloBarClicked); };

            adjustButton.Clicked += () => { RaiseIfNotInConfirmationState(AdjustClicked); };
            adjustCancelButton.Clicked += () => { AdjustCancelClicked.RaiseEvent(); };
            adjustConfirmButton.Clicked += () => { AdjustConfirmClicked.RaiseEvent(); };

            removeButton.Clicked += () => { RaiseIfNotInConfirmationState(RemoveClicked); };
            removeCancelButton.Clicked += () => { RemoveCancelClicked.RaiseEvent(); };
            removeConfirmButton.Clicked += () => { RemoveConfirmClicked.RaiseEvent(); };

            hideHoloBarButton.Clicked += () => { RaiseIfNotInConfirmationState(HideHoloBarClicked); };

            // Sets up the voice commands
            keywords.Add(VoiceCommandsManager.ShowHoloBarKeyword, () => { RaiseIfNotInConfirmationState(ShowHoloBarClicked); });
            keywords.Add(VoiceCommandsManager.AdjustKeyword, () => { RaiseIfNotInConfirmationState(AdjustClicked); });
            keywords.Add(VoiceCommandsManager.RemoveKeyword, () => { RaiseIfNotInConfirmationState(RemoveClicked); });
            keywords.Add(VoiceCommandsManager.HideHoloBarKeyword, () => { RaiseIfNotInConfirmationState(HideHoloBarClicked); });
            keywords.Add(VoiceCommandsManager.CancelKeyword, () =>
            {
                AdjustCancelClicked.RaiseEvent();
                RemoveCancelClicked.RaiseEvent();
            });
            keywords.Add(VoiceCommandsManager.ConfirmKeyword, () =>
            {
                AdjustConfirmClicked.RaiseEvent();
                RemoveConfirmClicked.RaiseEvent();
            });

            // Attaches the holobar flow methods to the buttons
            AdjustClicked += () => { ChangeBar(Constants.AdjustConfirmationBar); };
            AdjustCancelClicked += () => { ChangeBar(Constants.SouvenirBar); };
            AdjustConfirmClicked += () => { ChangeBar(Constants.SouvenirBar); };
            RemoveCancelClicked += () => { ChangeBar(Constants.SouvenirBar); };
            RemoveClicked += () => { ChangeBar(Constants.RemoveConfirmationBar); };
            RemoveConfirmClicked += () => { ChangeBar(Constants.SouvenirBar); };
            ShowHoloBarClicked += () => { ChangeBar(Constants.SouvenirBar); };
            HideHoloBarClicked += () => { ChangeBar(Constants.HiddenBar); };
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
            ChangeBar(Constants.AdjustConfirmationBar);
            AdjustClicked.RaiseEvent();
        }

        /// <summary>
        /// When the parent gets the focus, brings up the holobar if it was hidden.
        /// </summary>
        protected override void ShowHoloBar_OnParentFocusEnter()
        {
            if (currentStateName == null)
            {
                ChangeBarWithDelay(Constants.HiddenBar, Constants.ShowHoloBarDelay);
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
            return currentStateName == Constants.AdjustConfirmationBar || currentStateName == Constants.RemoveConfirmationBar;
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
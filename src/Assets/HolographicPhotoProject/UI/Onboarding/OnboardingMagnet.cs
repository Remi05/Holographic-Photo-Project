// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using HolographicPhotoProject.Data.RoamingProfiles;
using HolographicPhotoProject.Input.VoiceCommands;
using HolographicPhotoProject.Sound;
using HolographicPhotoProject.UI.AdjustmentBox;
using HolographicPhotoProject.UI.HoloBar;
using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_UWP
using HolographicPhotoProject.Data.Onedrive.Controllers;
using HolographicPhotoProject.Utilities;
using Microsoft.Practices.Unity;
#else
using HolographicPhotoProject.Souvenirs;
#endif

namespace HolographicPhotoProject.UI.Onboarding
{
    /// <summary>
    /// Component that ties together the HoloBar and its functionality for the Onboarding Magnet.
    /// </summary>
    public class OnboardingMagnet : MonoBehaviour, IInputClickHandler
    {
        private static class Constants
        {
            public const string OnboardingImagePath = "Images/onboarding";
            public const string DidShowOnboardingPlane = "DidShowOnboardingPlane";
            public const float InitialOnboardingMagnetDistance = 3f;
            public const float ConfirmationHoloBarEnableDelay = 0.5f;
            public const float PlacingStoppedShowOnboardingDelay = 1f;
        }

        [Tooltip("Type plane, that presents the onboarding image.")]
        public GameObject OnboardingPlane;

        [Tooltip("Text mesh that shows helper text when placing menu pin.")]
        public GameObject AirTapText;

        [Tooltip("Tooltip for find menu pin voice command.")]
        public GameObject PlaceMenuPinTooltip;

        /// <summary>
        /// Contains the voice commands and their callback.
        /// Key : Keyword
        /// Value : callback
        /// </summary>
        private Dictionary<string, Action> voiceCommands;

        private Adjustable adjustableComponent;
        private EventAudioManager eventAudioManager;
        private OnboardingMagnetHoloBar holobar;
        private Placeable placeableComponent;
        private RoamingDataManager roamingDataManager;
        private VoiceCommandsManager voiceCommandsManager;

        private string LoggedInUser
        {
            get
            {
#if UNITY_UWP
                if (OneDriveController.Instance != null)
                {
                    return OneDriveController.Instance.GetUserId();
                }
#endif
                return null;
            }
        }

        /// <summary>
        /// Blocks the click capture event from reaching the menu pin during
        /// initial placement, if the gaze is on the menu pin
        /// </summary>
        private bool isPlacingMagnet;

        private void Start()
        {
            SetOnboardingPlaneActive(false);
            SetAirTapTextActive(false);
            SetPlaceMenuPinTooltipActive(true);

            gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * Constants.InitialOnboardingMagnetDistance;

            InitAdjustableComponent();
            InitAudioManager();
            InitHoloBar();
            InitPlaceableComponent();
            InitRoamingDataManager();
            InitVoiceCommands();
        }

        /// <summary>
        /// Performs initialization related to the adjusting component.
        /// </summary>
        private void InitAdjustableComponent()
        {
            adjustableComponent = gameObject.GetComponent<Adjustable>();
            if (adjustableComponent != null)
            {
                adjustableComponent.TargetGameObject = gameObject;
            }
            else
            {
                Debug.LogError("Adjustable component not found.");
            }
        }

        /// <summary>
        /// Performs initialization related to the audio manager.
        /// </summary>
        private void InitAudioManager()
        {
            eventAudioManager = EventAudioManager.Instance;
            if (eventAudioManager == null)
            {
                Debug.LogError("AudioManager is not initialized.");
            }
        }

        /// <summary>
        /// Performs initialization related to the HoloBar.
        /// </summary>
        private void InitHoloBar()
        {
            holobar = gameObject.GetComponentInChildren<OnboardingMagnetHoloBar>(true);
            if (holobar != null)
            {
                // Bind the placer view to the HoloBar events.
                BindFunctionalityView<PlacerView>();

                // Bind the adjustment functionality component to the HoloBar events.
                if (adjustableComponent != null)
                {
                    holobar.AdjustClicked += adjustableComponent.StartAdjusting;
                    holobar.CancelClicked += adjustableComponent.CancelChanges;
                    holobar.ConfirmClicked += adjustableComponent.ConfirmChanges;
                }
            }
            else
            {
                Debug.LogError("Could not find OnboardingMagnetHoloBar component in children.");
            }
        }

        /// <summary>
        /// Performs initialization related to the placing component.
        /// </summary>
        private void InitPlaceableComponent()
        {
            placeableComponent = gameObject.GetComponentInChildren<Placeable>(true);
            if (placeableComponent != null)
            {
                placeableComponent.StartedPlacing += OnStartedPlacingMagnet;
                placeableComponent.StoppedPlacing += OnStoppedPlacingMagnet;
                placeableComponent.StartPlacing(false);
            }
        }

        /// <summary>
        /// Performs initialization related to roaming data manager.
        /// </summary>
        private void InitRoamingDataManager()
        {
#if UNITY_UWP
            IUnityContainer container = UnityDIContainer.Get();
            roamingDataManager = container?.Resolve<RoamingDataManager>();
            if (roamingDataManager == null)
            {
                Debug.LogError("Could not resolve an instance of RoamingDataManager in OnboardingMagnet.");
            }
#endif
        }

        /// <summary>
        /// Performs initialization related to the voice commands.
        /// </summary>
        private void InitVoiceCommands()
        {
            voiceCommands = new Dictionary<string, Action>();
            voiceCommandsManager = FindObjectOfType<VoiceCommandsManager>();
            if (voiceCommandsManager != null)
            {
                voiceCommands.Add(VoiceCommandsManager.PlaceMenuPinKeyword, BindToUser);
                voiceCommandsManager.SpeechKeywordRecognized += OnSpeechKeywordRecognized;
            }
            else
            {
                Debug.LogError("OnboardingMagnet.cs expects an instance of VoiceCommandsManager in the scene.");
            }
        }

        // TODO: as part of code refactoring, we should try to reuse the below code
        // from `Souvenir.cs`
        /// <summary>
        /// Binds an adjustment functionality view of the given type to the HoloBar events.
        /// </summary>
        private void BindFunctionalityView<T>() where T : MonoBehaviour
        {
            var adjustmentViewComponent = gameObject.GetComponentInChildren<T>(true);
            if (holobar != null && adjustmentViewComponent != null)
            {
                adjustmentViewComponent.gameObject.SetActive(false);
                holobar.AdjustClicked += () => { adjustmentViewComponent.gameObject.SetActive(true); };
                holobar.CancelClicked += () => { adjustmentViewComponent.gameObject.SetActive(false); };
                holobar.ConfirmClicked += () => { adjustmentViewComponent.gameObject.SetActive(false); };
            }
        }

        /// <summary>
        /// Binds the onboarding magnet to the user's gaze.
        /// </summary>
        private void BindToUser()
        {
            if (Placeable.ExistsObjectInPlacingMode)
            {
                return;
            }

            if (adjustableComponent != null && placeableComponent != null)
            {
                /// We shouldn't need to call `StartAdjusting`, however, at times,
                /// the pin doesn't attach to the gaze if this isn't called.
                adjustableComponent.StartAdjusting();
                placeableComponent.StartPlacing(false);
            }
            else
            {
                Debug.LogError("Onboarding expects the components Adjustable and Placeable but either was not found.");
            }
        }

        /// <summary>
        /// Enables the HoloBar if it exists.
        /// </summary>
        private void EnableHoloBar()
        {
            if (holobar != null)
            {
                holobar.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("No holobar for menu pin found.");
            }
        }

        /// <summary>
        /// Checks whether the user has finished onboarding or not.
        /// </summary>
        private bool IsUserFinishedOnboarding()
        {
            if (roamingDataManager == null)
            {
#if UNITY_UWP
                Debug.LogError("Roaming data manager is null.");
#endif
                return false;
            }

            if (LoggedInUser == null)
            {
#if UNITY_UWP

                Debug.LogError("No logged in user found.");
#endif
                return false;
            }

            try
            {
                var userOnboardingData = roamingDataManager.LoadData<Dictionary<string, object>>(Constants.DidShowOnboardingPlane);
                if (userOnboardingData == null)
                {
                    return false;
                }

                object doneOnboarding;
                userOnboardingData.TryGetValue(LoggedInUser, out doneOnboarding);

                return Convert.ToBoolean(doneOnboarding);
            }
            catch (System.ArgumentNullException ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
            catch (FormatException ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
            catch (InvalidCastException ex)
            {
                Debug.LogError(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// On click event for the magnet model object.
        /// </summary>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (isPlacingMagnet)
            {
                return;
            }

#if UNITY_UWP
            // On a real Hololens device, we'll navigate to the `FlatApp`.
            FlatAppHelper.NavigateToFlatApp();
#else
            // Alternatively, on the Unity Editor since we do not have access to the `FlatApp`,
            // we'll generate a default souvenir for debugging purposes.
            var souvenirManager = FindObjectOfType<SouvenirManager>();
            souvenirManager.CreateSouvenir("0", "PalmTree", string.Empty);
#endif
        }

        /// <summary>
        /// Invoked when the placing of the magnet object has started.
        /// </summary>
        private void OnStartedPlacingMagnet()
        {
            isPlacingMagnet = true;
            holobar.gameObject.SetActive(false);
            SetAirTapTextActive(true);
            SetPlaceMenuPinTooltipActive(false);
        }

        /// <summary>
        /// On placing stopped, show the onboarding plane.
        /// </summary>
        private void OnPlacingStoppedShowOnboarding()
        {
            isPlacingMagnet = false;

            if (IsUserFinishedOnboarding())
            {
                return;
            }

            // If the menu pin is in 'full adjustment' mode (i.e. bounding box is showing) and the placing
            // stops we don't want to show the onboarding plane until the 'full adjustment' stops.
            if (adjustableComponent.IsAdjusting)
            {
                return;
            }

            SetOnboardingPlaneActive(true);

            if (LoggedInUser == null)
            {
#if UNITY_UWP
                Debug.LogError("No logged in user found.");
#endif

                return;
            }

            if (roamingDataManager == null)
            {
#if UNITY_UWP
                Debug.LogError("Roaming data manager is null.");
#endif
                return;
            }

            var previousData = roamingDataManager.LoadData<Dictionary<string, object>>(Constants.DidShowOnboardingPlane);
            if (previousData != null)
            {
                previousData.Add(LoggedInUser, true);
            }
            else
            {
                previousData = new Dictionary<string, object> { { LoggedInUser, true } };
            }

            roamingDataManager.SaveData<Dictionary<string, object>>(Constants.DidShowOnboardingPlane, previousData);
        }

        /// <summary>
        /// If the parent is being gazed at and the keyword is in the keyword dictionary, invokes the corresponding callback.
        /// </summary>
        private void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
        {
            Debug.Log(String.Format("Keywords recognized : {0}", eventData.RecognizedText));

            Action action;
            if (voiceCommands.TryGetValue(eventData.RecognizedText.ToLower(), out action))
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Invoked when the placing of the magnet object has stopped.
        /// </summary>
        private void OnStoppedPlacingMagnet()
        {
            Invoke("EnableHoloBar", Constants.ConfirmationHoloBarEnableDelay);

            SetAirTapTextActive(false);
            SetPlaceMenuPinTooltipActive(true);

            if (eventAudioManager != null)
            {
                eventAudioManager.PlayAudio(EventAudioManager.TriggerType.SouvenirPlacement);
            }

            /// When the user taps to place the menu pin down and display
            /// the onboarding plane, it is shown too fast and the onboarding
            /// plane also captures the click event. The OnInputClick event is
            /// triggered which instantly opens the flat app so the user
            /// cannot read the onboarding instructions. We delay opening
            /// the onboarding plane to avoid it capturing the first tap event.
            Invoke("OnPlacingStoppedShowOnboarding", Constants.PlacingStoppedShowOnboardingDelay);
        }

        /// <summary>
        /// Toggles the AirTapText active state.
        /// </summary>
        /// <param name="active">If true, text shows, else it's hidden.</param>
        private void SetAirTapTextActive(bool active)
        {
            if (AirTapText != null)
            {
                AirTapText.SetActive(active);
            }
            else
            {
                Debug.LogError("AirTapText not found.");
            }
        }

        /// <summary>
        /// Toggles the onboarding plane active state.
        /// </summary>
        /// <param name="active">If true, plane shows, else it's hidden.</param>
        private void SetOnboardingPlaneActive(bool active)
        {
            if (OnboardingPlane != null)
            {
                OnboardingPlane.SetActive(active);
            }
            else
            {
                Debug.LogError("Onboarding plane not found.");
            }
        }

        /// <summary>
        /// Sets the tooltip's active state.
        /// </summary>
        private void SetPlaceMenuPinTooltipActive(bool active)
        {
            if (PlaceMenuPinTooltip != null)
            {
                PlaceMenuPinTooltip.SetActive(active);
            }
        }
    }
}
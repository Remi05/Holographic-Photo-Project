// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Input.Gaze;
using HolographicPhotoProject.Input.VoiceCommands;
using HolographicPhotoProject.Presenters.Presenters;
using HolographicPhotoProject.Presenters.Utilities;
using HolographicPhotoProject.Sound;
using HolographicPhotoProject.UI.AdjustmentBox;
using HolographicPhotoProject.UI.HoloBar;
using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_UWP
using HolographicPhotoProject.Data.Onedrive.Controllers;
using Microsoft.Practices.Unity;
#endif

namespace HolographicPhotoProject.Souvenirs
{
    /// <summary>
    /// Component that ties together the HoloBar and its functionality for a souvenir.
    /// </summary>
    public class Souvenir : MonoBehaviour, IInputClickHandler
    {
        private static class Constants
        {
            public const float ActiveStateTogglingEnableDelay = 0.5f;
            public const float ClearContentDelay = 2.0f;
            public const float ConfirmationHoloBarEnableDelay = 0.5f;
            public const float DisablePassiveStateDelay = 30.0f;
        }

        public enum SouvenirState { Off, Passive, Active }
        public event Action ActiveStateEnabled;
        public event Action ActiveStateDisabled;
        public event Action PassiveStateDisabled;
        public event Action PassiveStateEnabled;

        /// <summary>
        /// Provider for the content tied to the Souvenir (e.g. OneDrive content).
        /// </summary>
        public ContentProvider ContentProvider { get; set; }

        public string Id { get; set; }

        private bool CanBecomeActive { get { return !IsInActiveState && !IsBeingManipulated; } }
        private bool CanBecomePassive { get { return state == SouvenirState.Off && !IsBeingManipulated; } }
        private bool CanDisablePassive { get { return state == SouvenirState.Passive && !IsActiveAndGazed; } }
        private bool CanClearContent { get { return state == SouvenirState.Off; } }
        private bool IsActiveAndGazed
        {
            get
            {
                return gazeObserver.IsGazed
                       && activeSouvenirManager != null
                       && activeSouvenirManager.IsActiveSouvenir(this);
            }
        }
        private bool IsAdjusting { get { return adjustableComponent != null && adjustableComponent.IsAdjusting; } }
        private bool IsBeingManipulated { get { return IsAdjusting || IsPlacing; } }
        public bool IsInActiveState { get { return state == SouvenirState.Active; } }
        private bool IsPlacing { get { return placeableComponent != null && placeableComponent.IsPlacing; } }

        /// <summary>
        /// Contains the voice commands and their callback.
        /// Key : Keyword
        /// Value : callback
        /// </summary>
        protected Dictionary<string, Action> voiceCommands;

        private ActiveSouvenirManager activeSouvenirManager;
        private GameObject activeStateVisuals;
        private Adjustable adjustableComponent;
        private EventAudioManager eventAudioManager;
        private GazeObserver gazeObserver;
        private SouvenirHoloBar holoBar;
        private Placeable placeableComponent;
        private SpatialMappingManager spatialMappingManager;
        private VisibilityObserver visibilityObserver;
        private VoiceCommandsManager voiceCommandsManager;

        private SouvenirState state = SouvenirState.Off;

        private bool canToggleActiveState = true;
        private bool isInitialized = false;
        private bool removingAsked = false;
        private bool shouldStartPlacing = false;

        private void Start()
        {
            activeSouvenirManager = FindObjectOfType<ActiveSouvenirManager>();
            spatialMappingManager = FindObjectOfType<SpatialMappingManager>();

            // The order of these function calls is important because
            // the initialization related to certain components requires
            // other components to be initialized.
            InitActiveStateVisuals();
            InitAudioManager();
            InitAdjustableComponent();
            InitHoloBar();
            InitPlacableComponent();
            InitGazeObserver();
            InitVisibilityObserver();
            InitVoiceCommands();

            isInitialized = true;
        }

        public void Update()
        {
            if (shouldStartPlacing)
            {
                StartPlacing();
                shouldStartPlacing = false;
            }

            if (activeSouvenirManager != null
                && !activeSouvenirManager.IsActiveSouvenir(this))
            {
                Sleep();
            }
        }

        private void OnDestroy()
        {
            ClearContent();

            if (voiceCommandsManager != null)
            {
                voiceCommandsManager.SpeechKeywordRecognized -= OnSpeechKeywordRecognized;
            }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            ToggleActiveState();
        }

        /// <summary>
        /// If the parent is being gazed at and the keyword is in the keyword dictionary, invokes the corresponding callback.
        /// </summary>
        private void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
        {
            if (gazeObserver != null && gazeObserver.IsGazed)
            {
                Debug.Log(String.Format("Keywords recognized : {0}", eventData.RecognizedText));

                Action action;
                if (voiceCommands.TryGetValue(eventData.RecognizedText.ToLower(), out action))
                {
                    action.Invoke();
                }
            }
        }

        /// <summary>
        /// Binds the adjustment box views to the HoloBar events.
        /// </summary>
        private void BindAdjustmentBoxView()
        {
            BindFunctionalityView<PlacerView>();
            BindFunctionalityView<RotatorView>();
            BindFunctionalityView<ScalerView>();
        }

        /// <summary>
        /// Binds an adjustment functionality view of the given type to the HoloBar events.
        /// </summary>
        private void BindFunctionalityView<T>() where T : MonoBehaviour
        {
            var adjustmentViewComponent = gameObject.GetComponentInChildren<T>(true);
            if (holoBar != null && adjustmentViewComponent != null)
            {
                adjustmentViewComponent.gameObject.SetActive(false);
                holoBar.AdjustClicked += () => { adjustmentViewComponent.gameObject.SetActive(true); };
                holoBar.AdjustCancelClicked += () => { adjustmentViewComponent.gameObject.SetActive(false); };
                holoBar.AdjustConfirmClicked += () => { adjustmentViewComponent.gameObject.SetActive(false); };
            }
        }

        /// <summary>
        /// Performs initialization related to the active state visuals.
        /// </summary>
        private void InitActiveStateVisuals()
        {
            activeStateVisuals = TagsHelper.FindChildWithTag(gameObject, TagsHelper.ActiveStateVisualsTag);
            if (activeStateVisuals == null)
            {
                Debug.LogError("Could not find the active state visuals child GameObject in the Souvenir.");
            }
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
                adjustableComponent.AnchorId = Id;
                adjustableComponent.AdjustingStarted += ForceSleep;
            }
            else
            {
                Debug.LogError("Souvenir.cs expects an HSAdjustable component.");
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
                Debug.LogError("Souvenir.cs expects an instance of AudioManager in the scene.");
            }
        }

        /// <summary>
        /// Performs initialization related to the gaze observer.
        /// </summary>
        private void InitGazeObserver()
        {
            gazeObserver = GetComponent<GazeObserver>();
            if (gazeObserver != null)
            {
                gazeObserver.FocusEntered += EnablePassiveState;
            }
            else
            {
                Debug.LogError("Souvenir.cs expects a GazeObserver component.");
            }
        }

        /// <summary>
        /// Performs initialization related to the HoloBar.
        /// </summary>
        private void InitHoloBar()
        {
            holoBar = gameObject.GetComponentInChildren<SouvenirHoloBar>(true);
            if (holoBar != null)
            {
                holoBar.RemoveClicked += StartRemoving;
                holoBar.RemoveCancelClicked += CancelRemoving;
                holoBar.RemoveConfirmClicked += CommitRemoving;

                // Bind the adjustment view to the HoloBar events.
                BindAdjustmentBoxView();

                // Bind the adjustment functionality component to the HoloBar events.
                // The audio manager global listener is turned on or off when we need to 
                // stop playing the default air tap (i.e. when we are adjusting).
                if (adjustableComponent != null)
                {
                    holoBar.AdjustClicked += () =>
                    {
                        adjustableComponent.StartAdjusting();
                        eventAudioManager.TurnOffGlobalListener();
                    };
                    holoBar.AdjustCancelClicked += () =>
                    {
                        adjustableComponent.CancelChanges();
                        eventAudioManager.TurnOnGlobalListener();
                        eventAudioManager.PlayAudio();
                    };
                    holoBar.AdjustConfirmClicked += () =>
                    {
                        adjustableComponent.ConfirmChanges();
                        eventAudioManager.TurnOnGlobalListener();
                        eventAudioManager.PlayAudio();
                    };

                    adjustableComponent.AdjustingStarted += DisableActiveState;
                }
            }
            else
            {
                Debug.LogError("Souvenir.cs expects an HSHoloBar component in its children.");
            }
        }

        /// <summary>
        /// Performs initialization related to the placing component.
        /// </summary>
        private void InitPlacableComponent()
        {
            placeableComponent = gameObject.GetComponentInChildren<Placeable>(true);
            if (placeableComponent != null)
            {
                if (holoBar != null)
                {
                    placeableComponent.StartedPlacing += () => { holoBar.gameObject.SetActive(false); };
                    placeableComponent.StoppedPlacing += () =>
                    {
                        // The HoloBar is enabled with a delay to prevent double-clicking
                        // from confirming or canceling the adjustment when we click to stop placing.
                        Invoke("EnableHoloBar", Constants.ConfirmationHoloBarEnableDelay);
                    };
                }

                placeableComponent.StoppedPlacing += () =>
                {
                    // We enable active state toggling with a delay after placing
                    // to prevent double-clicking from activating the Souvenir when we click
                    // to stop the initial placement.
                    Invoke("EnableActiveStateToggle", Constants.ActiveStateTogglingEnableDelay);

                    if (eventAudioManager != null)
                    {
                        eventAudioManager.PlayAudio(EventAudioManager.TriggerType.SouvenirPlacement);
                    };

                    if (WorldAnchorManager.Instance != null
                        && adjustableComponent != null
                        && !adjustableComponent.IsAdjusting)
                    {
                        WorldAnchorManager.Instance.AttachAnchor(gameObject, Id);
                    }
                };
            }
            else
            {
                Debug.LogError("Souvenir.cs expects an Placeable component in its children.");
            }
        }

        /// <summary>
        /// Performs initialization related to the visibility observer.
        /// </summary>
        private void InitVisibilityObserver()
        {
            visibilityObserver = GetComponent<VisibilityObserver>();
            if (visibilityObserver != null)
            {
                visibilityObserver.BecameInvisible += () =>
                {
                    Invoke("DisablePassiveStateIfInvisible", Constants.DisablePassiveStateDelay);
                };
            }
            else
            {
                Debug.LogError("Souvenir.cs expects a GazeObserver component.");
            }
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
                voiceCommands.Add(VoiceCommandsManager.ExpandKeyword, EnableActiveState);
                voiceCommands.Add(VoiceCommandsManager.CollapseKeyword, DisableActiveState);
                voiceCommandsManager.SpeechKeywordRecognized += OnSpeechKeywordRecognized;
            }
            else
            {
                Debug.LogError("Souvenir.cs expects an instance of VoiceCommandsManager in the scene.");
            }
        }

        /// <summary>
        /// Starts placing of the souvenir.
        /// </summary>
        public void StartPlacing()
        {
            if (isInitialized)
            {
                if (placeableComponent != null)
                {
                    canToggleActiveState = false;
                    if (WorldAnchorManager.Instance != null && WorldAnchorManager.Instance.AnchorStore != null)
                    {
                        WorldAnchorManager.Instance.RemoveAnchor(gameObject);
                    }
                    placeableComponent.StartPlacing(false);
                }
            }
            else
            {
                shouldStartPlacing = true;
            }
        }

        /// <summary>
        /// Sets a flag indicating the user requested a deletion.
        /// </summary>
        private void StartRemoving()
        {
            removingAsked = true;
        }

        /// <summary>
        /// Sets a flag indicating the user requested a deletion.
        /// </summary>
        private void CancelRemoving()
        {
            removingAsked = false;
        }

        /// <summary>
        /// If a deletion was requested, destroys the souvenir gameobject.
        /// </summary>
        private void CommitRemoving()
        {
            if (removingAsked)
            {
#if UNITY_UWP
                IUnityContainer container = UnityDIContainer.Get();
                SouvenirRepository souvenirRepository = container.Resolve<SouvenirRepository>();
                SouvenirManager souvenirManager = FindObjectOfType<SouvenirManager>();
                souvenirRepository.RemoveSouvenir(souvenirManager.LoggedInUser, Id);
#endif
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Enables toggling of active state.
        /// </summary>
        /// <remarks>
        /// This is done in a method and not a lambda because
        /// Unity's Invoke() requires a named function.
        /// </remarks>
        private void EnableActiveStateToggle()
        {
            canToggleActiveState = true;
        }

        /// <summary>
        /// Enables the Souvenir Holobar.
        /// </summary> 
        /// <remarks>
        /// This is done in a method and not a lambda because
        /// Unity's Invoke() requires a named function.
        /// </remarks>
        private void EnableHoloBar()
        {
            if (holoBar != null)
            {
                holoBar.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Enables the updates to spatial mapping.
        /// </summary>
        private void EnableSpatialMapping()
        {
            if (spatialMappingManager != null)
            {
                spatialMappingManager.StartObserver();
            }
        }

        /// <summary>
        /// Disables the updates to spatial mapping.
        /// </summary>
        private void DisableSpatialMapping()
        {
            if (spatialMappingManager != null)
            {
                spatialMappingManager.StopObserver();
            }
        }

        /// <summary>
        /// Wakes up the souvenir, putting it in passive state.
        /// </summary>
        private void EnablePassiveState()
        {
            if (CanBecomePassive && activeSouvenirManager != null)
            {
                bool becamePassive = activeSouvenirManager.TryMakePassive(this);
                if (becamePassive)
                {
                    ShowContent();
                    state = SouvenirState.Passive;
                    PassiveStateEnabled.RaiseEvent();
                }
            }
        }

        /// <summary>
        /// Disables the souvenir, puts it in sleeping state.
        /// </summary>
        private void DisablePassiveState()
        {
            if (!CanDisablePassive)
            {
                return;
            }

            if (activeSouvenirManager != null)
            {
                activeSouvenirManager.MakeInactive(this);
            }

            activeStateVisuals.SetActive(false);
            state = SouvenirState.Off;
            PassiveStateDisabled.RaiseEvent();
        }

        private void ForceSleep()
        {
            DisableActiveState();

            // Force disable passive.
            if (activeSouvenirManager != null)
            {
                activeSouvenirManager.MakeInactive(this);
            }

            activeStateVisuals.SetActive(false);
            state = SouvenirState.Off;
            PassiveStateDisabled.RaiseEvent();
        }

        /// <summary>
        /// Disables the souvenir, puts it in sleeping state
        /// if it is not currently in the camera's field of view.
        /// </summary>
        private void DisablePassiveStateIfInvisible()
        {
            if (visibilityObserver != null && !visibilityObserver.IsVisible)
            {
                DisablePassiveState();
            }
        }

        /// <summary>
        /// Toggle between active and inactive state of the souvenir.
        /// </summary>
        public void ToggleActiveState()
        {
            if (canToggleActiveState && !IsAdjusting)
            {
                if (state == SouvenirState.Active)
                {
                    DisableActiveState();
                }
                else
                {
                    EnableActiveState();
                }
            }
        }

        /// <summary>
        /// Puts the Souvenir in an active state.
        /// </summary>
        private void EnableActiveState()
        {
            if (CanBecomeActive)
            {
                if (activeSouvenirManager != null)
                {
                    activeSouvenirManager.MakeActive(this);
                }

                DisableSpatialMapping();
                ShowContent();
                activeStateVisuals.SetActive(true);
                eventAudioManager.PlayAudio(EventAudioManager.TriggerType.SouvenirActive);
                state = SouvenirState.Active;
                ActiveStateEnabled.RaiseEvent();
            }
        }

        /// <summary>
        /// Puts the Souvenir in an passive state if it is currently active.
        /// </summary>
        private void DisableActiveState()
        {
            if (state != SouvenirState.Active)
            {
                return;
            }

            activeStateVisuals.SetActive(false);
            eventAudioManager.PlayAudio(EventAudioManager.TriggerType.SouvenirInactive);
            EnableSpatialMapping();
            state = SouvenirState.Passive;
            ActiveStateDisabled.RaiseEvent();
        }

        /// <summary>
        /// Puts the Souvenir in a sleeping state.
        /// </summary>
        public void Sleep()
        {
            DisableActiveState();
            DisablePassiveState();
        }

        /// <summary>
        /// Shows content provided by the content provider
        /// visible on the children content presenter.
        /// </summary>
        private void ShowContent()
        {
            var childrenPresenters = GetComponentsInChildren<ImagePresenter>();
            foreach (var presenter in childrenPresenters)
            {
                if (presenter.ContentAccessor == null)
                {
                    presenter.ContentAccessor = ContentProvider.GetNewAccessor();
                }
                presenter.ShowContent();
            }
        }

        /// <summary>
        /// Hides the content presented on the children content presenters.
        /// </summary>
        private void ClearContent()
        {
            if (!CanClearContent)
            {
                return;
            }

            var childrenPresenters = GetComponentsInChildren<ImagePresenter>();
            foreach (var presenter in childrenPresenters)
            {
                presenter.ClearContent();
                presenter.HideContent();
            }
        }
    }
}
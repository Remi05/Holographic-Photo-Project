// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using HolographicPhotoProject.Input.Gaze;
using HolographicPhotoProject.Input.VoiceCommands;
using HolographicPhotoProject.Presenters.Presenters;
using HolographicPhotoProject.Souvenirs;
using HolographicPhotoProject.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HolographicPhotoProject.Sound;

namespace HolographicPhotoProject.Presenters.Sources
{
    [RequireComponent(typeof(GazeObserver))]
    public class CarouselPresenterSource : PresenterSource
    {
        private static class Constants
        {
            public const string ImagePlaneMaterial = "Materials/ImagePlane";
            public const string PhotoArrangementPrefab = "Prefabs/PhotoArrangement";
            public const string NextArrow = "NextArrow";
            public const string PreviousArrow = "PreviousArrow";
            public const string LoadingIcon = "LoadingIcon";

            public const float CarouselRadius = 0.10f;
            public const int MaxNumberOfPresenters = 6;

            public const float TransitionDuration = 5;

            public const float PresenterRotationSpeedMin = 0.05f;
            public const float PresenterRotationSpeedMax = 0.5f;

            public static readonly Vector3 DefaultPlaneScale = new Vector3(0.005f, 0.005f, 0.005f);
            public static readonly Vector3 DefaultPlanePosition = new Vector3(0, 0, 0.1f);
            public static readonly Vector3 PlaneRotationCorrection = new Vector3(-90.0f, 0.0f, 0.0f);

            public static readonly Vector3 ScaleRandomMin = new Vector3(0.0015f, 0.0015f, 0.0015f);
            public static readonly Vector3 ScaleRandomMax = new Vector3(0.003f, 0.003f, 0.003f);

            public static readonly Vector3 PositionRandomMin = new Vector3(0.0f, -0.1f, -0.03f);
            public static readonly Vector3 PositionRandomMax = new Vector3(0.0f, 0.1f, 0.05f);
        }

        /// <summary>
        /// The souvenir associated with this presenter source.
        /// </summary>
        public Souvenir Souvenir;

        private GameObject carousel;
        private GameObject photoArrangement;
        private List<GameObject> photoCoordinates;

        private GameObject photoNavigator;
        private PhotoNavigatorButton navigatorNextButton;
        private PhotoNavigatorButton navigatorPreviousButton;

        private List<CarouselSlotBehavior> carouselSlots = new List<CarouselSlotBehavior>();
        private ActiveSouvenirManager presenterManager;
        private int numberOfPresenters;

        private static GameObject photoArrangementPrefab;
        private static Material planeMaterial;

        private bool isInContentTransition;
        private bool shouldDisplayLoadedContent;

        private GameObject loadingIcon;
        private bool IsLoadingIconActive { get { return loadingIcon == null ? false : loadingIcon.activeSelf; } }
        
        private Dictionary<string, Action> voiceCommands;
        private GazeObserver gazeObserver;

        private EventAudioManager eventAudioManager;

        private void Start()
        {
            gazeObserver = GetComponent<GazeObserver>();
            voiceCommands = new Dictionary<string, Action>();
            eventAudioManager = EventAudioManager.Instance;

            planeMaterial = Resources.Load<Material>(Constants.ImagePlaneMaterial);
            if (planeMaterial == null)
            {
                Debug.LogError("Could not load the image plane material.");
            }

            presenterManager = FindObjectOfType<ActiveSouvenirManager>();
            if (presenterManager == null)
            {
                Debug.LogError("This script expects a presenter manager component in the scene.");
            }

            if (photoArrangementPrefab == null)
            {
                photoArrangementPrefab = Resources.Load<GameObject>(Constants.PhotoArrangementPrefab);
                if (photoArrangementPrefab == null)
                {
                    Debug.LogError("Could not load photo arrangement prefab.");
                }
            }

            if (photoArrangementPrefab != null)
            {
                photoArrangement = Instantiate(photoArrangementPrefab, transform);

                if (photoArrangement != null)
                {
                    // We want the photo arrangement to be centered with the souvenir.
                    photoArrangement.transform.localPosition = Vector3.zero;
                    photoArrangement.transform.localEulerAngles = Vector3.zero;

                    photoCoordinates = TagsHelper.FindChildrenWithTag(photoArrangement, TagsHelper.ArrangementPictureCoords);
                    photoNavigator = TagsHelper.FindChildWithTag(photoArrangement, TagsHelper.PhotoNavigator);
                    loadingIcon = TagsHelper.FindChildWithTag(photoArrangement, Constants.LoadingIcon);

                    numberOfPresenters = Math.Min(Constants.MaxNumberOfPresenters, photoCoordinates.Count);
                }
                else
                {
                    Debug.LogError("Could not instantiate the photo arrangement prefab.");
                }              
            }

            CreatePresenters();

            BindPhotoNavigator();

            if (Souvenir != null)
            {
                carousel.transform.parent = Souvenir.transform;

                Souvenir.ActiveStateEnabled += EnableGridState;
                Souvenir.ActiveStateDisabled += EnableCarouselState;
                Souvenir.PassiveStateEnabled += EnableCarouselState;
                Souvenir.PassiveStateDisabled += EnableSleepingState;
            }

            isInContentTransition = true;
        }

        private void OnDestroy()
        {
            foreach (var carouselSlot in carouselSlots)
            {
                Destroy(carouselSlot);
            }

            var voiceCommandsManager = FindObjectOfType<VoiceCommandsManager>();
            if (voiceCommandsManager != null)
            {
                voiceCommandsManager.SpeechKeywordRecognized -= OnSpeechKeywordRecognized;
            }

            Destroy(carousel);
        }

        private void Update()
        {
            UpdatePosition();
            UpdateImages();
        }

        private void UpdateImages()
        {
            // Checks if there are images ready to be displayed.
            if (shouldDisplayLoadedContent)
            {
                SetLoadingIconActive(false);
                foreach (var p in presenters)
                {
                    var presenter = p as ImagePresenter;
                    if (presenter != null)
                    {
                        presenter.DisplayLoadedContent();
                    }
                }
                if (Souvenir.IsInActiveState)
                {
                    EnablePhotoNavigation();
                }
                shouldDisplayLoadedContent = false;
            }

            // If the presenters are loading, checks if they are loaded.
            if (isInContentTransition)
            {
                if (!IsLoadingIconActive && Souvenir.IsInActiveState)
                {
                    SetLoadingIconActive(true);
                    DisablePhotoNavigation();
                }

                foreach (var p in presenters)
                {
                    var presenter = p as ImagePresenter;
                    if (presenter == null || !presenter.IsContentReady)
                    {
                        return;
                    }
                }
                shouldDisplayLoadedContent = true;
                isInContentTransition = false;
            }
        }

        private void UpdatePosition()
        {
            if (Souvenir == null || carousel == null)
            {
                return;
            }

            carousel.transform.localPosition = transform.InverseTransformVector(new Vector3(0, GetGameObjectHeight(Souvenir.gameObject), 0));
        }

        private void CreatePresenters()
        {
            carousel = new GameObject("Carousel");

            for (int i = 0; i < numberOfPresenters; ++i)
            {
                GameObject carouselSlot = new GameObject("CarouselSlot");
                carouselSlot.transform.parent = carousel.transform;
                carouselSlot.transform.localPosition = Vector3.zero;
                carouselSlot.transform.localRotation = Quaternion.Euler(0.0f, i * 360.0f / numberOfPresenters, 0.0f);
                var carouselSlotBehavior = carouselSlot.AddComponent<CarouselSlotBehavior>();
                carouselSlotBehavior.RotationSpeed = UnityEngine.Random.Range(Constants.PresenterRotationSpeedMin, Constants.PresenterRotationSpeedMax);
                carouselSlots.Add(carouselSlotBehavior);
                carouselSlotBehavior.Clicked += () => { ToggleZoomedState(carouselSlotBehavior); };

                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.tag = TagsHelper.InteractableTag;
                plane.AddComponent<BoxCollider>();
                plane.transform.parent = carouselSlot.transform;
                plane.transform.localScale = Constants.DefaultPlaneScale + GetRandomVector(Constants.ScaleRandomMin, Constants.ScaleRandomMax);
                plane.transform.localPosition = Constants.DefaultPlanePosition + GetRandomVector(Constants.PositionRandomMin, Constants.PositionRandomMax);
                plane.transform.localRotation = Quaternion.Euler(Constants.PlaneRotationCorrection);

                var renderer = plane.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = planeMaterial;
                }

                var presenter = plane.AddComponent<ImagePresenter>();
                carouselSlotBehavior.Presenter = presenter;
                presenters.Add(presenter);
            }

            UpdatePosition();
            UpdateImages();
        }

        private void ToggleZoomedState(CarouselSlotBehavior carouselSlotBehavior)
        {
            if (isInContentTransition)
            {
                return;
            }

            if (eventAudioManager != null)
            {
                eventAudioManager.PlayAudio();
            }

            var isZoomed = carouselSlotBehavior.IsZoomed;

            if (Souvenir.IsInActiveState)
            {
                UpdateGrid();
            }
            else
            {
                Souvenir.ToggleActiveState();
            }

            if (!isZoomed)
            {
                var yOffset = GetGameObjectHeight(Souvenir.gameObject);
                var zoomedCoordsObject = TagsHelper.FindChildWithTag(photoArrangement, TagsHelper.ZoomedPictureCoords);
                var position = transform.InverseTransformPoint(zoomedCoordsObject.transform.position)
                               - transform.InverseTransformVector(new Vector3(0, yOffset, 0));

                carouselSlotBehavior.EnableZoomedMode(position);
            }
        }

        private void SetupGrid()
        {
            photoArrangement.transform.LookAt(Camera.main.transform);
            photoArrangement.transform.eulerAngles = new Vector3(0, photoArrangement.transform.eulerAngles.y, 0);

            var photoArrangementPosition = photoArrangement.transform.position;
            float arrangementHeight = Mathf.Max(GetGameObjectHeight(Souvenir.gameObject) + Souvenir.transform.position.y + 0.25f, Camera.main.transform.position.y);
            photoArrangement.transform.position = new Vector3(photoArrangementPosition.x, arrangementHeight, photoArrangementPosition.z);

            ShowPhotoNavigator();
        }

        private void UpdateGrid()
        {
            var yOffset = GetGameObjectHeight(Souvenir.gameObject);

            var numberOfDisplayedSlots = Math.Min(carouselSlots.Count, photoCoordinates.Count);
            for (int i = 0; i < numberOfDisplayedSlots; ++i)
            {
                var carouselSlot = carouselSlots[i];

                // We transform the world position of the grid slot to local coordinates, then we remove the height of the souvenir 
                // to remove the offset. The height must be transformed to local coordinates too, because of the scaling factor.
                var position = transform.InverseTransformPoint(photoCoordinates[i].transform.position)
                               - TransformsHelper.WorldToLocalScale(carouselSlot.transform, new Vector3(0, yOffset, 0));

                carouselSlot.EnableGridMode(position);
            }
        }

        private void EnableGridState()
        {
            SetupGrid();
            UpdateGrid();
        }

        private void EnableCarouselState()
        {
            HidePhotoNavigator();
            SetLoadingIconActive(false);
            carouselSlots.ForEach(s => s.EnableCarouselMode());
        }

        private void EnableSleepingState()
        {
            HidePhotoNavigator();
            SetLoadingIconActive(false);
            carouselSlots.ForEach(s => s.EnableSleepingMode());
        }

        public void BindPhotoNavigator()
        {
            if (photoNavigator != null)
            {
                var nextArrow = photoNavigator.transform.FindChild(Constants.NextArrow);
                var previousArrow = photoNavigator.transform.FindChild(Constants.PreviousArrow);
                if (nextArrow == null || previousArrow == null)
                {
                    Debug.LogError("The next or previous arrow is null.");
                    return;
                }

                navigatorNextButton = nextArrow.gameObject.GetComponent<PhotoNavigatorButton>();
                navigatorPreviousButton = previousArrow.gameObject.GetComponent<PhotoNavigatorButton>();
                if (navigatorNextButton == null || navigatorPreviousButton == null)
                {
                    Debug.LogError("Navigation Arrow buttons cannot be found.");
                    return;
                }

                navigatorNextButton.Clicked += OnNextClicked;
                navigatorPreviousButton.Clicked += OnPreviousClicked;

                var voiceCommandsManager = FindObjectOfType<VoiceCommandsManager>();
                if (voiceCommandsManager != null)
                {
                    voiceCommands.Add(VoiceCommandsManager.NextKeyword, OnNextClicked);
                    voiceCommands.Add(VoiceCommandsManager.PreviousKeyword, OnPreviousClicked);
                    voiceCommandsManager.SpeechKeywordRecognized += OnSpeechKeywordRecognized;
                }
                else
                {
                    Debug.LogError("Souvenir.cs expects an instance of VoiceCommandsManager in the scene.");
                }
            }
        }

        private void OnNextClicked()
        {
            if (isInContentTransition || !Souvenir.IsInActiveState)
            {
                return;
            }

            isInContentTransition = true;
            DisablePhotoNavigation();
            SetLoadingIconActive(true);

            // We place the presenters in grid mode, in case one of them was zoomed.
            UpdateGrid();

            foreach (var p in presenters)
            {
                var presenter = p as ContentPresenter<Texture2D>;
                if (presenter != null)
                {
                    presenter.LoadNext(numberOfPresenters);
                }
            }
        }

        private void OnPreviousClicked()
        {
            if (isInContentTransition || !Souvenir.IsInActiveState)
            {
                return;
            }

            isInContentTransition = true;
            DisablePhotoNavigation();
            SetLoadingIconActive(true);

            // We place the presenters in grid mode, in case one of them was zoomed.
            UpdateGrid();

            foreach (var p in presenters)
            {
                var presenter = p as ContentPresenter<Texture2D>;
                if (presenter != null)
                {
                    presenter.LoadPrevious(numberOfPresenters);
                }
            }
        }
        
        private void SetLoadingIconActive(bool active)
        {
            if (loadingIcon == null)
            {
                return;
            }
            loadingIcon.SetActive(active);
        }
                
        private void DisablePhotoNavigation()
        {
            navigatorNextButton.DisableNavigatorButton();
            navigatorPreviousButton.DisableNavigatorButton();
        }

        private void EnablePhotoNavigation()
        {
            navigatorNextButton.EnableNavigatorButton();
            navigatorPreviousButton.EnableNavigatorButton();
        }

        private void ShowPhotoNavigator()
        {
            navigatorNextButton.Show();
            navigatorPreviousButton.Show();
            EnablePhotoNavigation();
        }

        private void HidePhotoNavigator()
        {
            navigatorNextButton.Hide();
            navigatorPreviousButton.Hide();
        }
        
        private Vector3 GetRandomVector(Vector3 min, Vector3 max)
        {
            return new Vector3(UnityEngine.Random.Range(min.x, max.x),
                               UnityEngine.Random.Range(min.y, max.y),
                               UnityEngine.Random.Range(min.z, max.z));
        }

        public override HashSet<IContentPresenter> GetPresenters()
        {
            return presenters;
        }

        private float GetGameObjectHeight(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<Collider>()
                             .Where(c => c.GetComponent<IContentPresenter>() == null)
                             .Max(c => c.bounds.extents.y) * 2;
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
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;
using HolographicPhotoProject.Sound;

namespace HolographicPhotoProject.Presenters.Presenters
{
    [RequireComponent(typeof(AnimationExecutor))]
    public class PhotoNavigatorButton : MonoBehaviour, IInputClickHandler
    {
        private static class Constants
        {
            public const float ClickAnimationDuration = 0.3f;
            public const float ToggleAnimationDuration = 1f;
            public const float MagnifyingFactor = 1.01f;

            public const int FrontMaterialIndex = 1;
            
            public static readonly Vector3 DefaultScale = new Vector3(0.07f, 0.07f, 0.07f);
            public static readonly Vector3 DisabledScale = new Vector3(0.06f, 0.06f, 0.06f);
        }

        /// <summary>
        /// Triggered when the user clicks on the button.
        /// </summary>
        public event Action Clicked;
        
        private bool isEnabled = true;

        private EventAudioManager eventAudioManager;

        private AnimationExecutor animationExecutor;
        private KeyFrame hiddenKeyFrame;
        private KeyFrame shownKeyFrame;
        private KeyFrame disabledKeyFrame;
        
        public Material EnabledMaterial;
        public Material DisabledMaterial;
        public GameObject ArrowGameObject;
        
        public void Start()
        {
            animationExecutor = GetComponent<AnimationExecutor>();

            shownKeyFrame = AnimationExecutor.SaveKeyFrame(gameObject);
            hiddenKeyFrame = AnimationExecutor.SaveKeyFrame(gameObject);
            hiddenKeyFrame.LocalScale = Vector3.zero;
            disabledKeyFrame = AnimationExecutor.SaveKeyFrame(gameObject);
            disabledKeyFrame.LocalScale = Constants.DisabledScale;

            AnimationExecutor.ApplyKeyFrame(gameObject, hiddenKeyFrame);

            eventAudioManager = EventAudioManager.Instance;
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            if (!isEnabled)
            {
                return;
            }

            if (eventAudioManager != null)
            {
                eventAudioManager.PlayAudio();
            }

            Clicked.RaiseEvent();
        }

        public void EnableNavigatorButton()
        {
            MaterialsHelper.ChangeMaterial(ArrowGameObject.transform, EnabledMaterial, Constants.FrontMaterialIndex);
            animationExecutor.StartAnimation(shownKeyFrame, Constants.ClickAnimationDuration);
            isEnabled = true;
        }

        public void DisableNavigatorButton()
        {
            MaterialsHelper.ChangeMaterial(ArrowGameObject.transform, DisabledMaterial, Constants.FrontMaterialIndex);
            animationExecutor.StartAnimation(disabledKeyFrame, Constants.ClickAnimationDuration);
            isEnabled = false;
        }

        public void Show()
        {
            animationExecutor.StartAnimation(shownKeyFrame, Constants.ToggleAnimationDuration);
        }

        public void Hide()
        {
            animationExecutor.StartAnimation(hiddenKeyFrame, Constants.ToggleAnimationDuration);
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Based on the GestureSoundManager.cs script from the Windows Holographic Academy course, Holograms 220 Spatial Sound.
// Src : https://github.com/Microsoft/HolographicAcademy/blob/Holograms-220-SpatialSound/Completed/Decibel/Assets/Scripts/GestureSoundManager.cs

using System;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using UnityEngine;

namespace HolographicPhotoProject.Sound
{
    /// <summary>
    /// Allows audio to be played for user interactions in the HoloLens.
    /// </summary>
    public class EventAudioManager : Singleton<EventAudioManager>, IInputClickHandler
    {
        // The enum here is duplicated in EventAudioHandler.
        public enum TriggerType
        {
            AirTap,
            SouvenirPlacement,
            SouvenirActive,
            SouvenirInactive,
            Count
        }

        private EventAudioHandler eventAudioHandler;
        private AudioSource audioSource;
        private AudioClip audioClip;

        protected void Start()
        {
            eventAudioHandler = gameObject.GetComponent<EventAudioHandler>();
            audioSource = gameObject.EnsureComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.volume = 0.2f;

            // We want playing of audio to be handled if no other event takes care of it.
            InputManager.Instance.PushFallbackInputHandler(gameObject);
        }

        /// <summary>
        /// Handler called when the user clicks in the environment.
        /// </summary>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            PlayAudio();
        }

        /// <summary>
        /// Allows the audio clip to be assigned.
        /// </summary>
        public void SetAudioClip(TriggerType triggerType)
        {
            if (eventAudioHandler != null)
            {
                // Fetch the appropriate audio clip from the EventAudioHandler's AudioClips array.
                audioClip = eventAudioHandler.AudioClips[(int)triggerType];
            }
        }

        /// <summary>
        /// Assigns the audio source clip to the default or specific clip and plays it.
        /// At the end, the audio clip is cleared.
        /// </summary>
        public void PlayAudio()
        {
            if (audioClip == null)
            {
                audioClip = eventAudioHandler.AudioClips[(int)eventAudioHandler.DefaultAudioClip];
            }
            audioSource.clip = audioClip;
            audioSource.Play();

            // The audio clip will be reset to the default.
            audioClip = null;
        }

        /// <summary>
        /// Assigns the audio clip based on the trigger type and plays the audio
        /// </summary>
        public void PlayAudio(TriggerType triggerType)
        {
            SetAudioClip(triggerType);
            PlayAudio();
        }

        public void TurnOnGlobalListener()
        {
            InputManager.Instance.AddGlobalListener(gameObject);
        }

        public void TurnOffGlobalListener()
        {
            InputManager.Instance.RemoveGlobalListener(gameObject);
        }
    }
}
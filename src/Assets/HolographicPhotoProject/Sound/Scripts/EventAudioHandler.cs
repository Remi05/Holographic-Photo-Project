// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// Based on the GestureSoundHandler.cs script from the Windows Holographic Academy course, Holograms 220 Spatial Sound.
// Src : https://github.com/Microsoft/HolographicAcademy/blob/Holograms-220-SpatialSound/Completed/Decibel/Assets/Scripts/GestureSoundHandler.cs

using UnityEngine;

namespace HolographicPhotoProject.Sound
{
    /// <summary>
    /// Allows audio clips to be injected through Unity for easier access in the code.
    /// </summary>
    public class EventAudioHandler : MonoBehaviour
    {
        public AudioClip AirTapClip;
        public AudioClip SouvenirPlacementClip;
        public AudioClip SouvenirActiveClip;
        public AudioClip SouvenirInactiveClip;

        // The enum here is a duplicate of EventAudioManager.
        // This enum is only used to expose the options for default audio clip to the Unity editor.
        public enum TriggerType
        {
            AirTap,
            SouvenirPlacement,
            SouvenirActive,
            SouvenirInactive
        }
        [Tooltip("The default audio clip to be used for audio feedback.")]
        public TriggerType DefaultAudioClip = TriggerType.AirTap;

        public AudioClip[] AudioClips
        {
            get; private set;
        }

        private void Awake()
        {
            // Make it super convenient for designers to specify sounds in the UI 
            // and developers to access specific sounds in code.
            AudioClips = new AudioClip[(int)EventAudioManager.TriggerType.Count];
            AudioClips[(int)EventAudioManager.TriggerType.AirTap] = AirTapClip;
            AudioClips[(int)EventAudioManager.TriggerType.SouvenirPlacement] = SouvenirPlacementClip;
            AudioClips[(int)EventAudioManager.TriggerType.SouvenirActive] = SouvenirActiveClip;
            AudioClips[(int)EventAudioManager.TriggerType.SouvenirInactive] = SouvenirInactiveClip;
        }
    }
}
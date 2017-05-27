// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Input.Gaze;
using HolographicPhotoProject.Input.VoiceCommands;
using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HolographicPhotoProject.UI.HoloBar
{
    public abstract class HoloBar : MonoBehaviour
    {
        /// <summary>
        /// Contains the voice commands and their callback.
        /// Key : Keyword
        /// Value : callback
        /// </summary>
        protected Dictionary<string, Action> keywords;

        /// <summary>
        /// The current state of the holobar.
        /// </summary>
        protected string currentStateName;

        /// <summary>
        /// The state in which the holobar will be after the next call to RefreshBar.
        /// </summary>
        protected string nextStateName;

        protected virtual void Start()
        {
            keywords = new Dictionary<string, Action>();

            // Hides the holobar if the parent object is not on focus
            GazeObserver gazeObserver = transform.GetComponentInParent<GazeObserver>();
            if (gazeObserver != null)
            {
                gazeObserver.FocusEntered += ShowHoloBar_OnParentFocusEnter;
                gazeObserver.FocusExited += HideHoloBar_OnParentFocusExit;
            }

            var voiceCommandManager = FindObjectOfType<VoiceCommandsManager>();
            if (voiceCommandManager != null)
            {
                voiceCommandManager.SpeechKeywordRecognized += OnSpeechKeywordRecognized;
            }

            RefreshBar();
        }

        private void OnDestroy()
        {
            GazeObserver gazeObserver = transform.GetComponentInParent<GazeObserver>();

            if (gazeObserver != null)
            {
                gazeObserver.FocusEntered -= ShowHoloBar_OnParentFocusEnter;
                gazeObserver.FocusExited -= HideHoloBar_OnParentFocusExit;
            }

            var voiceCommandManager = FindObjectOfType<VoiceCommandsManager>();
            if (voiceCommandManager != null)
            {
                voiceCommandManager.SpeechKeywordRecognized -= OnSpeechKeywordRecognized;
            }
        }

        /// <summary>
        /// Changes the state of the holobar to the given state after a given delay.
        /// </summary>
        /// <param name="delay">The delay in seconds.</param>
        protected void ChangeBarWithDelay(string stateName, float delay)
        {
            nextStateName = stateName;
            Invoke("RefreshBar", delay);
        }

        /// <summary>
        /// Changes the state of the holobar to the given state.
        /// </summary>
        /// <param name="stateName"></param>
        protected void ChangeBar(string stateName)
        {
            nextStateName = stateName;
            RefreshBar();
        }

        /// <summary>
        /// Changes the state of the holobar to nextStateName.
        /// </summary>
        protected void RefreshBar()
        {
            foreach (Transform child in transform)
            {
                bool shouldBeActive = String.Equals(child.name, nextStateName);
                child.gameObject.SetActive(shouldBeActive);
            }

            currentStateName = nextStateName;
        }

        /// <summary>
        /// If the parent is being gazed at and the keyword is in the keyword dictionnary, invokes the corresponding callback.
        /// </summary>
        private void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
        {
            GazeObserver gazeObserver = transform.GetComponentInParent<GazeObserver>();

            if (gazeObserver != null && gazeObserver.IsGazed)
            {
                Debug.Log(String.Format("Keywords recognized : {0}", eventData.RecognizedText));

                Action action;
                if (keywords.TryGetValue(eventData.RecognizedText.ToLower(), out action))
                {
                    action.Invoke();
                }
            }
        }

        /// <summary>
        /// Opens the flat app.
        /// </summary>
        protected void SwitchToFlatApp()
        {
            FlatAppHelper.NavigateToFlatApp();
        }

        /// <summary>
        /// When the parent gets the focus, brings up the holobar if it was hidden.
        /// </summary>
        protected abstract void ShowHoloBar_OnParentFocusEnter();

        /// <summary>
        /// When the parent loses the focus, hides the holobar after a delay defined by Constants.HideHoloBarDelay.
        /// </summary>
        protected abstract void HideHoloBar_OnParentFocusExit();
    }
}

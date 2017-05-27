// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HolographicPhotoProject.Input.VoiceCommands
{
    /// <summary>
    /// Voice commands in this class are triggered anywhere. They are not affected by the gaze.
    /// </summary>
    public class GeneralVoiceCommands : MonoBehaviour
    {
        private Dictionary<string, Action> keywords;

        private void Start()
        {
            keywords = new Dictionary<string, Action>();

            var voiceCommandsManager = FindObjectOfType<VoiceCommandsManager>();
            if (voiceCommandsManager != null)
            {
                voiceCommandsManager.SpeechKeywordRecognized += OnSpeechKeywordRecognized;
            }

            keywords.Add(VoiceCommandsManager.OpenMenuKeyword, () => { FlatAppHelper.NavigateToFlatApp(); });
            keywords.Add(VoiceCommandsManager.ToggleMeshKeyword, () => {
                if (SpatialMappingManager.IsInitialized)
                {
                    SpatialMappingManager.Instance.DrawVisualMeshes = !SpatialMappingManager.Instance.DrawVisualMeshes;
                }
            });
        }

        private void OnDestroy()
        {
            var voiceCommandsManager = FindObjectOfType<VoiceCommandsManager>();
            if (voiceCommandsManager != null)
            {
                voiceCommandsManager.SpeechKeywordRecognized -= OnSpeechKeywordRecognized;
            }
        }

        /// <summary>
        /// If the parent is being gazed at and the keyword is in the keyword dictionary, invokes the corresponding callback.
        /// </summary>
        private void OnSpeechKeywordRecognized(SpeechKeywordRecognizedEventData eventData)
        {
            Debug.Log(String.Format("Keywords recognized : {0}", eventData.RecognizedText));

            Action action;
            if (keywords.TryGetValue(eventData.RecognizedText.ToLower(), out action))
            {
                action.Invoke();
            }
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows.Speech;

namespace HolographicPhotoProject.Input.VoiceCommands
{
    /// <summary>
    /// Triggers an event when a voice commands is recognized. Also contains a list of 
    /// all the available voice commands.
    /// </summary>
    public class VoiceCommandsManager : BaseInputSource
    {
        /// <summary>
        /// Represents a voice command keyword and it's keycode.
        /// </summary>
        public struct KeywordAndKeyCode
        {
            [Tooltip("The keyword to recognize.")]
            public string Keyword;
            [Tooltip("The KeyCode to recognize.")]
            public KeyCode KeyCode;
        }

        public event Action<SpeechKeywordRecognizedEventData> SpeechKeywordRecognized;

        private List<KeywordAndKeyCode> keywords;
        private KeywordRecognizer keywordRecognizer;
        private SpeechKeywordRecognizedEventData speechKeywordRecognizedEventData;

        /// <summary>
        /// List of the available voice commands. If you add a voice command here,
        /// don't forget to add it also to the Keyword dictionary in the start method.
        /// </summary>
        public const string AdjustKeyword = "adjust";
        public const string CancelKeyword = "cancel";
        public const string ConfirmKeyword = "confirm";
        public const string PlaceMenuPinKeyword = "place menu pin";
        public const string HideHoloBarKeyword = "hide holobar";
        public const string NextKeyword = "next";
        public const string OpenMenuKeyword = "open menu";
        public const string PreviousKeyword = "previous";
        public const string RemoveKeyword = "remove";
        public const string ShowHoloBarKeyword = "show holobar";
        public const string StartSlideshowKeyword = "start slideshow";
        public const string StopSlideshowKeyword = "stop slideshow";
        public const string ExpandKeyword = "expand";
        public const string CollapseKeyword = "collapse";
        public const string ToggleMeshKeyword = "toggle spatial mesh";

        protected override void Start()
        {
            base.Start();

            speechKeywordRecognizedEventData = new SpeechKeywordRecognizedEventData(EventSystem.current);

            keywords = new List<KeywordAndKeyCode>
            {
                new KeywordAndKeyCode { Keyword = AdjustKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = CancelKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = ConfirmKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = PlaceMenuPinKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = HideHoloBarKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = NextKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = OpenMenuKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = PreviousKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = RemoveKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = ShowHoloBarKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = StartSlideshowKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = StopSlideshowKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = ExpandKeyword, KeyCode = 0 },
                new KeywordAndKeyCode { Keyword = CollapseKeyword, KeyCode = 0 }
            };

            int keywordCount = keywords.Count;
            if (keywordCount > 0)
            {
                SetupKeywordRecognizer();
                keywordRecognizer.Start();
            }
        }

        protected virtual void Update()
        {
            if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                ProcessKeyBindings();
            }
        }

        protected virtual void OnDestroy()
        {
            if (keywordRecognizer != null)
            {
                StopKeywordRecognizer();
                keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
                keywordRecognizer.Dispose();
            }
        }

        protected virtual void OnDisable()
        {
            if (keywordRecognizer != null)
            {
                StopKeywordRecognizer();
            }
        }

        protected virtual void OnEnable()
        {
            if (keywordRecognizer != null)
            {
                StartKeywordRecognizer();
            }
        }

        private void ProcessKeyBindings()
        {
            foreach (var keyword in keywords)
            {
                if (UnityEngine.Input.GetKeyDown(keyword.KeyCode))
                {
                    OnPhraseRecognized(ConfidenceLevel.High, TimeSpan.Zero, DateTime.Now, null, keyword.Keyword);
                }
            }
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            OnPhraseRecognized(args.confidence, args.phraseDuration, args.phraseStartTime, args.semanticMeanings, args.text);
        }

        /// <summary>
        /// Makes sure the keyword recognizer is off, then starts it.
        /// Otherwise, leaves it alone because it's already in the desired state.
        /// </summary>
        public void StartKeywordRecognizer()
        {
            if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Start();
            }
        }

        /// <summary>
        /// Makes sure the keyword recognizer is on, then stops it.
        /// Otherwise, leaves it alone because it's already in the desired state.
        /// </summary>
        public void StopKeywordRecognizer()
        {
            if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }
        }

        /// <summary>
        /// Creates a new keywordRecognizer.
        /// Useful for refreshing the keywords if some were added.
        /// </summary>
        private void SetupKeywordRecognizer()
        {
            if (keywordRecognizer != null)
            {
                keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;
                keywordRecognizer = null;
            }

            keywordRecognizer = new KeywordRecognizer(keywords.Select(k => k.Keyword).ToArray());
            keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        }

        /// <summary>
        /// Raises an event to indicate a speech keyword was recognized.
        /// </summary>
        private void OnPhraseRecognized(ConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, SemanticMeaning[] semanticMeanings, string text)
        {
            // Create input event
            speechKeywordRecognizedEventData.Initialize(this, 0, confidence, phraseDuration, phraseStartTime, semanticMeanings, text);
            SpeechKeywordRecognized(speechKeywordRecognizedEventData);
        }

        public override bool TryGetPosition(uint sourceId, out Vector3 position)
        {
            position = Vector3.zero;
            return false;
        }

        public override bool TryGetOrientation(uint sourceId, out Quaternion orientation)
        {
            orientation = Quaternion.identity;
            return false;
        }

        public override SupportedInputInfo GetSupportedInputInfo(uint sourceId)
        {
            return SupportedInputInfo.None;
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HolographicPhotoProject.Input.Gestures
{
    public class Clickable : MonoBehaviour, IInputHandler, IInputClickHandler
    {
        /// <summary>
        /// Triggered when the object is pressed.
        /// </summary>
        public event Action Pressed;

        /// <summary>
        /// Triggered when the object is released.
        /// </summary>
        public event Action Released;

        /// <summary>
        /// Triggered when the object is tapped upon.
        /// </summary>
        public event Action Clicked;

        /// <summary>
        /// Contains the event data.
        /// </summary>
        public InputEventData EventData { get; protected set; }

        /// <summary>
        /// The input source from which the OnInputDown event was last captured.
        /// </summary>
        public IInputSource InputSource { get { return EventData.InputSource; } }

        /// <summary>
        /// The ID of the input source from which the OnInputDown event was last captured.
        /// </summary>
        public uint InputSourceId { get { return EventData.SourceId; } }

        /// <summary>
        /// Handler for the OnInputDown event which forwards this event up as Pressed.
        /// </summary>
        public void OnInputDown(InputEventData eventData)
        {
            EventData = eventData;
            Pressed.RaiseEvent();
        }

        /// <summary>
        /// Handler for the OnInputUp event which forwards this event up as Released.
        /// </summary>
        public void OnInputUp(InputEventData eventData)
        {
            // Only trigger Released event if the input
            // source is the same as when it was pressed down.
            if (eventData.SourceId == InputSourceId)
            {
                Released.RaiseEvent();
            }
        }

        public void OnInputClicked(InputClickedEventData eventData)
        {
            EventData = eventData;
            Clicked.RaiseEvent();
        }
    }
}

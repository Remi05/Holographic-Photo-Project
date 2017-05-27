// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HolographicPhotoProject.Input.Gestures
{
    public class IgnoreClick : MonoBehaviour, IInputHandler, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData) { }

        public void OnInputDown(InputEventData eventData) { }

        public void OnInputUp(InputEventData eventData) { }
    }
}

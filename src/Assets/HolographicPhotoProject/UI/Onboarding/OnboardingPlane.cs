// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using HolographicPhotoProject.Utilities;
using UnityEngine;

namespace HolographicPhotoProject.UI.Onboarding
{
    public class OnboardingPlane : MonoBehaviour, IInputClickHandler
    {
        public void OnInputClicked(InputClickedEventData eventData)
        {
            FlatAppHelper.NavigateToFlatApp();
            Destroy(gameObject);
        }
    }
}
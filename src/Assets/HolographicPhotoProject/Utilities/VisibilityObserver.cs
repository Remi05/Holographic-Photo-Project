// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using UnityEngine;

public class VisibilityObserver : MonoBehaviour
{
    public event Action BecameInvisible; 
    public event Action BecameVisible;

    private bool isVisible; 
    public bool IsVisible { get { return isVisible; } }

    private bool HasRenderers { get { return renderers != null && renderers.Length > 0; } } 

    private Renderer[] renderers;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        if (!HasRenderers)
        {
            Debug.Log("VisibilityObserver expects children renderers yet none were found.");
        }
    }

    private void Update()
    {
        if (HasRenderers)
        {
            bool isCurrentlyVisible = Array.Exists(renderers, r => r.isVisible);
            if (isCurrentlyVisible != isVisible)
            {
                isVisible = isCurrentlyVisible;
                if (isCurrentlyVisible)
                {
                    BecameVisible.RaiseEvent();
                }
                else
                {
                    BecameInvisible.RaiseEvent();
                }
            }
        }
    }
}

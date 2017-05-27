// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Presenters.Utilities;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;

namespace HolographicPhotoProject.Presenters.Presenters
{
    /// <summary>
    /// Defines the generic behaviour of a content presenter which presents
    /// content of the specified type onto holograms in the users environment.
    /// </summary>
    /// <typeparam name="T">Type of the content to present.</typeparam>
    public abstract class ContentPresenter<T> : MonoBehaviour, IContentPresenter, IInputClickHandler
    {
        /// <summary>
        /// Event raised the the presenter is clicked.
        /// </summary>
        public event Action Clicked;

        /// <summary>
        /// Event raised when the content is hidden.
        /// </summary>
        public virtual event Action ContentHidden;

        /// <summary>
        /// Event raised when the content is shown.
        /// </summary>
        public virtual event Action ContentShown;

        protected T currentResource;

        /// <summary>
        /// An accessor to the content to be presented.
        /// </summary>
        public IContentAccessor ContentAccessor { get; set; }

        /// <summary>
        /// Formatter for the resource data.
        /// </summary>
        public ContentFormatter<T> Formatter { get; set; }
        
        /// <summary>
        /// Indicates if the content is visible or not.
        /// </summary>
        public bool IsContentVisible { get; protected set; }

        /// <summary>
        /// Indicates whether the content is done loading or not.
        /// </summary>
        public bool IsContentReady { get; protected set; }
        
        protected virtual void OnDestroy()
        {
            HideContent();
        }

        protected virtual void OnDisable()
        {
            HideContent();
        }
        
        /// <summary>
        /// Cleans up any content held by the presenter.
        /// </summary>
        public virtual void ClearContent()
        {
            ContentAccessor = null;
        }

        /// <summary>
        /// Displays the given resource.
        /// </summary>
        protected virtual void OnContentReady(T resource)
        {
            currentResource = resource;
            IsContentReady = true;
        }

        public abstract void DisplayLoadedContent();
        
        /// <summary>
        /// Fetches the current resource.
        /// </summary>
        public virtual void LoadCurrent()
        {
            if (!IsContentVisible)
            {
                return;
            }

            if (ContentAccessor == null)
            {
                Debug.LogError("The presenter's content is null.");
                return;
            }

            if (Formatter == null)
            {
                Debug.LogError("The presenter's content formatter is null.");
            }

#if UNITY_UWP
            IsContentReady = false;
            ContentAccessor.GetCurrentResource<T>(Formatter, OnContentReady);
#endif
        }

        /// <summary>
        /// Fetches the next resource.
        /// </summary>
        public virtual void LoadNext(int offset = 1)
        {
            if (!IsContentVisible)
            {
                return;
            }

            if (ContentAccessor == null)
            {
                Debug.LogError("The presenter's content is null.");
                return;
            }

            if (Formatter == null)
            {
                Debug.LogError("The presenter's content formatter is null.");
            }

#if UNITY_UWP
            IsContentReady = false;
            ContentAccessor.GetNextResource<T>(Formatter, OnContentReady, offset);
#endif
        }

        /// <summary>
        /// Fetches the previous resource.
        /// </summary>
        public virtual void LoadPrevious(int offset = 1)
        {
            if (!IsContentVisible)
            {
                return;
            }

            if (ContentAccessor == null)
            {
                Debug.LogError("The presenter's content is null.");
                return;
            }

            if (Formatter == null)
            {
                Debug.LogError("The presenter's content formatter is null.");
            }

#if UNITY_UWP
            IsContentReady = false;
            ContentAccessor.GetPreviousResource<T>(Formatter, OnContentReady, offset);
#endif
        }

        /// <summary>
        /// Hides the content.
        /// </summary>
        public virtual void HideContent()
        {
            if (!IsContentVisible)
            {
                return;
            }

            IsContentVisible = false;
            ContentHidden.RaiseEvent();
        }

        /// <summary>
        /// Makes the content visible.
        /// </summary>
        public virtual void ShowContent()
        {
            if (IsContentVisible)
            {
                return;
            }

            IsContentVisible = true;
            LoadCurrent();

            ContentShown.RaiseEvent();
        }

        /// <summary>
        /// Handler for the InputClicked event which displays the next resource.
        /// </summary>
        public void OnInputClicked(InputClickedEventData eventData)
        {
            Clicked.RaiseEvent();
        }
    }
}
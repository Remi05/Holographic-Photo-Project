// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Presenters.Utilities;
using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity;
using System;
using UnityEngine;

namespace HolographicPhotoProject.Presenters.Presenters
{
    [RequireComponent(typeof(AnimationExecutor))]
    public class ImagePresenter : ContentPresenter<Texture2D>
    {
        private static class Constants
        {
            public const float TransitionDuration = 1.0f;
        }

        private float maxWidth;
        private float maxHeight;

        private AnimationExecutor animationExecutor;
        
        public override event Action ContentHidden;

        private void Awake()
        {
            Formatter = ContentFormatters.TextureFormatter;
        }

        protected virtual void Start()
        {
            maxWidth = transform.localScale.x;
            maxHeight = transform.localScale.z;
            animationExecutor = GetComponent<AnimationExecutor>();
        }

        /// <summary>
        /// Cleans up any content held by the presenter.
        /// </summary>
        public override void ClearContent()
        {
            base.ClearContent();

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Make sure we delete Unity resources to prevent memory leaks.
                var previousResource = renderer.material.mainTexture;
                if (previousResource != null)
                {
                    Destroy(previousResource);
                }
            }
        }

        public override void DisplayLoadedContent()
        {
            if (currentResource == null)
            {
                return;
            }

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("The presenter's parent GameObject doesn't have a Renderer component.");
                return;
            }

            // Make sure we delete Unity resources to prevent memory leaks.
            var previousResource = renderer.material.mainTexture;
            if (previousResource != null)
            {
                Destroy(previousResource);
            }

            UpdateImageSize(currentResource);
            renderer.material.mainTexture = currentResource;
        }

        /// <summary>
        /// Hides the content.
        /// </summary>
        public override void HideContent()
        {
            if (!IsContentVisible)
            {
                return;
            }

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = null;
            }

            IsContentVisible = false;
            ContentHidden.RaiseEvent();
        }
        
        /// <summary>
        /// Resizes the GameObject to fit the given image.
        /// </summary>
        private void UpdateImageSize(Texture2D image)
        {
            float imageWidth = image.width;
            float imageHeight = image.height;
            ImageUtils.LimitImageDimensionsTo(
                maxWidth,
                maxHeight,
                ref imageWidth,
                ref imageHeight);

            var newScale = new Vector3(imageWidth, 1f, imageHeight);
            
            KeyFrame keyFrame;
            keyFrame.LocalScale = newScale;
            keyFrame.LocalPosition = gameObject.transform.localPosition;
            keyFrame.LocalRotation = gameObject.transform.localRotation.eulerAngles;

            animationExecutor.StartAnimation(keyFrame, Constants.TransitionDuration);
        }
    }
}
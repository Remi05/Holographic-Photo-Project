// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity;
using System;
using UnityEngine;

namespace HolographicPhotoProject.Presenters.Presenters
{
    [RequireComponent(typeof(AnimationExecutor))]
    public class CarouselSlotBehavior : MonoBehaviour
    {
        private static class Constants
        {
            // The factor by which the photos are scaled when placed in grid mode.
            public const float GridStatePhotoScaleFactor = 3.5f;

            public const float TransitionDuration = 1.5f;
            public const float ZoomedPlaneDistance = 1.2f;

            public static readonly Vector3 PlaneRotationCorrection = new Vector3(-90f, 0f, 0f);
        }

        private enum State { Sleeping, Carousel, Grid, Zoomed }

        public event Action Clicked;

        /// <summary>
        /// Speed of the rotation.
        /// </summary>
        public float RotationSpeed;

        /// <summary>
        /// Tells if the presenter is in zoomed state.
        /// </summary>
        public bool IsZoomed { get { return state == State.Zoomed; } }

        public ImagePresenter Presenter;

        private KeyFrame sleepingKeyFrame;
        private KeyFrame carouselKeyFrame;
        private KeyFrame gridKeyFrame;
        private KeyFrame zoomedKeyFrame;

        private KeyFrame presenterSleepingKeyFrame;
        private KeyFrame presenterCarouselKeyFrame;
        private KeyFrame presenterGridKeyFrame;
        private KeyFrame presenterZoomedKeyFrame;

        private float currentRotationToCamera;

        private Vector3 gridWorldScale;

        private State state = State.Sleeping;

        private bool IsInAnimation
        {
            get
            {
                return animationExecutor != null && animationExecutor.IsInAnimation
                       || presenterAnimationExecutor != null && presenterAnimationExecutor.IsInAnimation;
            }
        }

        private AnimationExecutor animationExecutor;
        private AnimationExecutor presenterAnimationExecutor;

        private void Start()
        {
            animationExecutor = GetComponent<AnimationExecutor>();

            if (Presenter != null)
            {
                presenterCarouselKeyFrame = AnimationExecutor.SaveKeyFrame(Presenter.gameObject);
                presenterSleepingKeyFrame = AnimationExecutor.SaveKeyFrame(Presenter.gameObject);

                presenterAnimationExecutor = Presenter.GetComponent<AnimationExecutor>();

                Presenter.Clicked += () => { Clicked.RaiseEvent(); };
            }

            carouselKeyFrame = AnimationExecutor.SaveKeyFrame(gameObject);
            sleepingKeyFrame = AnimationExecutor.SaveKeyFrame(gameObject);

            sleepingKeyFrame.LocalScale = Vector3.zero;
            sleepingKeyFrame.LocalRotation = Constants.PlaneRotationCorrection + new Vector3(0, 0, 180);

            gridWorldScale = transform.lossyScale * Constants.GridStatePhotoScaleFactor;

            transform.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (state == State.Carousel)
            {
                transform.Rotate(new Vector3(0, RotationSpeed, 0));

                if (!IsInAnimation)
                {
                    Presenter.transform.localEulerAngles = Constants.PlaneRotationCorrection;
                }
            }
        }

        public void EnableGridMode(Vector3 position)
        {
            if (state == State.Grid)
            {
                return;
            }

            if (state != State.Zoomed)
            {
                var lookDirection = Camera.main.transform.position - transform.parent.position;
                currentRotationToCamera = Quaternion.LookRotation(lookDirection, Vector3.up).eulerAngles.y % 360;

                gridKeyFrame.LocalRotation = new Vector3(0, currentRotationToCamera, 0);
                gridKeyFrame.LocalScale = TransformsHelper.WorldToLocalScale(transform, gridWorldScale);
                gridKeyFrame.LocalPosition = position;
            }
            else
            {
                gridKeyFrame.LocalRotation = new Vector3(0, currentRotationToCamera, 0);
            }

            presenterGridKeyFrame.LocalPosition = Vector3.zero;
            presenterGridKeyFrame.LocalRotation = Constants.PlaneRotationCorrection;

            animationExecutor.StartAnimation(gridKeyFrame, Constants.TransitionDuration);
            presenterAnimationExecutor.StartAnimation(presenterGridKeyFrame,
                                                      Constants.TransitionDuration,
                                                      animateRotation: false,
                                                      animateScale: false);

            state = State.Grid;
        }

        public void EnableCarouselMode()
        {
            if (state == State.Carousel)
            {
                return;
            }

            animationExecutor.StartAnimation(carouselKeyFrame, Constants.TransitionDuration);
            presenterAnimationExecutor.StartAnimation(presenterCarouselKeyFrame,
                                                      Constants.TransitionDuration,
                                                      animateRotation: false,
                                                      animateScale: false);

            state = State.Carousel;
        }

        public void EnableSleepingMode()
        {
            if (state == State.Sleeping)
            {
                return;
            }

            animationExecutor.StartAnimation(sleepingKeyFrame, Constants.TransitionDuration);
            presenterAnimationExecutor.StartAnimation(presenterSleepingKeyFrame,
                                                      Constants.TransitionDuration,
                                                      animateRotation: false,
                                                      animateScale: false);

            state = State.Sleeping;
        }

        public void EnableZoomedMode(Vector3 position)
        {
            zoomedKeyFrame = gridKeyFrame;
            presenterZoomedKeyFrame = presenterGridKeyFrame;

            zoomedKeyFrame.LocalPosition = position;
            zoomedKeyFrame.LocalRotation = new Vector3(0, currentRotationToCamera, 0);
            zoomedKeyFrame.LocalScale = gridKeyFrame.LocalScale * 2;

            presenterGridKeyFrame.LocalPosition = Vector3.zero;
            presenterGridKeyFrame.LocalRotation = Constants.PlaneRotationCorrection;

            animationExecutor.StartAnimation(zoomedKeyFrame, Constants.TransitionDuration);
            presenterAnimationExecutor.StartAnimation(presenterZoomedKeyFrame,
                                                      Constants.TransitionDuration,
                                                      animateRotation: false,
                                                      animateScale: false);

            state = State.Zoomed;
        }
    }
}
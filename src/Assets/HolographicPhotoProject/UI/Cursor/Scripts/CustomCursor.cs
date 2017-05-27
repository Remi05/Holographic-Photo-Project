// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Utilities;
using HoloToolkit.Unity.InputModule;
using System.Linq;
using UnityEngine;

namespace HolographicPhotoProject.UI.Cursor
{
    public class CustomCursor : ObjectCursor
    {
        private static class Constants
        {
            /// <summary>
            /// Distance between the cursor object and the object the cursor is following.
            /// </summary>
            public const float CursorToFollowObjDistance = 0.05f;
        }

        // A generic gameobject that the cursor gets locked to.
        private GameObject cursorLockedObject;

        protected override void OnFocusedObjectChanged(GameObject previousObject, GameObject newObject)
        {
            base.OnFocusedObjectChanged(previousObject, newObject);
            UpdateCursor(CursorState, newObject);
        }

        public override void OnCursorStateChange(CursorStateEnum state)
        {
            GameObject gazedObject = null;
            if (GazeManager.IsInitialized)
            {
                gazedObject = GazeManager.Instance.HitObject;
            }

            UpdateCursor(state, gazedObject);
        }

        private void UpdateCursor(CursorStateEnum state, GameObject gazedObject)
        {
            if (state == CursorStateEnum.Select && cursorLockedObject == null)
            {
                cursorLockedObject = gazedObject;
            }
            else if (state == CursorStateEnum.Release)
            {
                cursorLockedObject = null;
            }

            base.OnCursorStateChange(state);

            // 1. For any game objects with tag `Interactable` the base class knows how to deal with them
            // so we won't do anything.
            // 2. If we have a game object under cursor lock, then we won't show our custom cursor icons, 
            // instead we'll let the parent class show the `Select` state icon.
            if ((gazedObject != null && gazedObject.tag != TagsHelper.InteractableTag) && cursorLockedObject == null)
            {
                SetCursorWithName(gazedObject.tag);
            }
        }

        private void SetCursorWithName(string name)
        {
            var currentStateCursor = new ObjectCursorDatum();
            foreach (var cursor in CursorStateData)
            {
                if (cursor.Name == name)
                {
                    currentStateCursor = cursor;
                    break;
                }
            }

            if (currentStateCursor.Name == null)
            {
                currentStateCursor = CursorStateData.FirstOrDefault();

                if (default(ObjectCursorDatum).Equals(currentStateCursor))
                {
                    return;
                }
            }

            // De-activate all of base class cursor states.
            foreach (var cursor in CursorStateData)
            {
                if (cursor.CursorObject && cursor.CursorObject.activeSelf)
                {
                    cursor.CursorObject.SetActive(false);
                    break;
                }
            }

            // Activate newer cursor.
            currentStateCursor.CursorObject.SetActive(true);
        }

        protected void LateUpdate()
        {
            // If hand is not in FOV of the hololens, it likely means the user cancelled the operation,
            // so if we're still stuck in `Select` mode, let's escape!
            if (!IsHandVisible && CursorState == CursorStateEnum.Select)
            {
                IsInputSourceDown = false;
            }

            if (cursorLockedObject == null)
            {
                return;
            }

            if (cursorLockedObject.tag == TagsHelper.ScalerTag
                || cursorLockedObject.tag == TagsHelper.RotatorTag)
            {
                // Keeps the cursor on the same side as the camera (always!).
                Vector3 lockedPos = cursorLockedObject.transform.position;
                lockedPos -= Camera.main.transform.forward * Constants.CursorToFollowObjDistance;
                gameObject.transform.position = lockedPos;
            }
        }
    }
}
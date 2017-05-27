// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace HolographicPhotoProject.Utilities
{
    public static class TagsHelper
    {
        /// <summary>
        /// An object marked with this tag is asking for the cursor to change depending on state.
        /// </summary>
        public const string InteractableTag = "Interactable";

        /// <summary>
        /// An object that is used to indicate the active state of another object.
        /// </summary>
        public const string ActiveStateVisualsTag = "ActiveStateVisuals";

        /// <summary>
        /// An object that is typically empty and used only for its position.
        /// </summary>
        public const string CoordinateTag = "Coord";

        /// <summary>
        /// Scale handles on the bounding box are marked with this tag.
        /// </summary>
        public const string ScalerTag = "Scaler";

        /// <summary>
        /// Rotate handles on the bounding box are marked with this tag.
        /// </summary>
        public const string RotatorTag = "Rotator";

        /// <summary>
        /// An object that is typically empty and used only for its position.
        /// </summary>
        public const string ArrangementPictureCoords = "ArrangementPictureCoords";

        /// <summary>
        /// An object that is typically empty and used only for its position.
        /// </summary>
        public const string ZoomedPictureCoords = "ZoomedPictureCoords";

        /// <summary>
        /// Photo navigator for iterating through displayed resources.
        /// </summary>
        public const string PhotoNavigator = "PhotoNavigator";

        public static List<GameObject> FindChildrenWithTag(GameObject gameObject, string tag)
        {
            if (gameObject == null)
            {
                return new List<GameObject>();
            }

            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.tag == tag)
                {
                    children.Add(child.gameObject);
                }
            }

            return children;
        }

        public static GameObject FindChildWithTag(GameObject gameObject, string tag)
        {
            if (gameObject == null)
            {
                return null;
            }

            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.tag == tag)
                {
                    return child.gameObject;
                }
            }

            return null;
        }
    }
}
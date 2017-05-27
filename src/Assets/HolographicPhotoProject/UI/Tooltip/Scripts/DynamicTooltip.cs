// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Input.Gaze;
using UnityEngine;
using UnityEngine.UI;

namespace HolographicPhotoProject.UI.Tooltip
{
    /// <summary>
    /// This component will show its game object when its parent is being gazed at for a certain amount of time.
    /// </summary>
    public class DynamicTooltip : Tooltip
    {
        private static class Constants
        {
            public const string DefaultText = "Say \"Adjust\"";
        }

        [Tooltip("Text displayed by the tooltip.")]
        public string Text = Constants.DefaultText;

        [Tooltip("Game object displaying the text label.")]
        public GameObject Label;

        [Tooltip("The text container.")]
        public Text TextCanvas;

        [Tooltip("The background of the tooltip.")]
        public GameObject Background;

        private bool isInit;
        private float widthRatio;

        private void Update()
        {
            var labelWidth = ((RectTransform)Label.transform).rect.width;

            // We have to do this in the update because the HorizontalLayoutGroup's width is not initialized yet when
            // the start is called.
            if (!isInit)
            {
                if (labelWidth == 0)
                {
                    return;
                }

                if (Background != null)
                {
                    // This takes a reference ratio to resize the background to fit the text. This ratio is relative 
                    // to the initial tooltips background and label initial sizes.
                    widthRatio = Background.transform.localScale.x / labelWidth;
                    isInit = true;
                }
            }

            TextCanvas.text = Text;

            if (Background != null)
            {
                // Adjusts the width of the background to fit the text length
                Background.transform.localScale = new Vector3(widthRatio * labelWidth, Background.transform.localScale.y, Background.transform.localScale.z);
            }
        }
    }
}

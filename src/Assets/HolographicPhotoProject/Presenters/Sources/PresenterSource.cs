// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Presenters.Presenters;
using System.Collections.Generic;
using UnityEngine;

namespace HolographicPhotoProject.Presenters.Sources
{
    /// <summary>
    /// Represents a source of content presenters (used to 
    /// populate the presenter manager's collection of presenters).
    /// </summary>
    public abstract class PresenterSource : MonoBehaviour
    {
        protected HashSet<IContentPresenter> presenters = new HashSet<IContentPresenter>();

        /// <summary>
        /// Returns the content presenters created by the source.
        /// </summary>
        public abstract HashSet<IContentPresenter> GetPresenters();

        /// <summary>
        /// Refreshes the presenter set held by the source (does nothing by default).
        /// </summary>
        public virtual void RefreshPresenters() { }

        /// <summary>
        /// Hides the presenters held by the source. Does nothing by default.
        /// </summary>
        public virtual void HidePresenters() { }

        /// <summary>
        /// Shows the presenters held by the source. Does nothing by default.
        /// </summary>
        public virtual void ShowPresenters() { }
    }
}
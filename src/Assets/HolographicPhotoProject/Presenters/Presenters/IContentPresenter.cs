// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Presenters.Utilities;

namespace HolographicPhotoProject.Presenters.Presenters
{
    /// <summary>
    /// Defines the core, content-type independant, functionalities of content presenters.
    /// </summary>
    public interface IContentPresenter
    {
        /// <summary>
        /// An accessor to the content to be presented.
        /// </summary>
        IContentAccessor ContentAccessor { get; set; }

        /// <summary>
        /// Clears the content held by the presenter.
        /// </summary>
        void ClearContent();

        /// <summary>
        /// Hides the content.
        /// </summary>
        void HideContent();

        /// <summary>
        /// Makes the content visible.
        /// </summary>
        void ShowContent();
    }
}
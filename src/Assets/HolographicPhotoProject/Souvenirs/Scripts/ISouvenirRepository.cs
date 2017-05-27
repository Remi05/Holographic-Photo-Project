// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Data.RoamingProfiles;
using System.Collections.Generic;

namespace HolographicPhotoProject.Souvenirs
{
    /// <summary>
    /// Interface for a repository.
    /// </summary>
    public interface ISouvenirRepository
    {
        List<SouvenirModel> LoadSouvenirs(string userId);

        void AddSouvenir(string userId, SouvenirModel data);

        void RemoveSouvenir(string userId, string dataId);
    }
}
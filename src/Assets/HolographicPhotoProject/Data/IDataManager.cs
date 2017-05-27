// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace HolographicPhotoProject.Data
{
    /// <summary>
    /// The interface for roaming app data.
    /// </summary>
    public interface IDataManager
    {
        T LoadData<T>(string propertyName);

        void SaveData<T>(string propertyName, T data);

        void Remove(string propertyName);
    }
}
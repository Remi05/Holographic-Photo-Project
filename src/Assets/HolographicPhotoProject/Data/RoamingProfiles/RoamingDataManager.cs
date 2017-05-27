// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_UWP
using Newtonsoft.Json;
using System.Diagnostics;
#endif

namespace HolographicPhotoProject.Data.RoamingProfiles
{
    /// <summary>
    /// This class handles the interactions with roaming app data.
    /// </summary>
    public class RoamingDataManager : IDataManager
    {
        /// <summary>
        /// Loads the roaming app data associated with the propertyName.
        /// </summary>
        public T LoadData<T>(string propertyName)
        {
#if UNITY_UWP
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            var value = roamingSettings.Values[propertyName];
            if (value != null)
            {
                T data = JsonConvert.DeserializeObject<T>(value.ToString());
                return data;
            }
#endif
            return default(T);
        }

        /// <summary>
        /// Saves the data into roaming app data.
        /// </summary>
        public void SaveData<T>(string propertyName, T data)
        {
#if UNITY_UWP
            string savingData = JsonConvert.SerializeObject(data);
            if (savingData == null)
            {
                Debug.WriteLine("Error: Data serialization failed.");
                return;
            }

            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            System.Object value = roamingSettings.Values[propertyName];
            roamingSettings.Values[propertyName] = savingData;
#endif
        }
        
        /// <summary>
        /// Removes the property from roaming app data.
        /// </summary>
        public void Remove(string propertyName)
        {
#if UNITY_UWP
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values.Remove(propertyName);
#endif
        }
    }
}

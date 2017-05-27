// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_UWP
using Newtonsoft.Json;
using System;
#endif
using HolographicPhotoProject.Data.RoamingProfiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HolographicPhotoProject.Data;

namespace HolographicPhotoProject.Souvenirs
{
    public class SouvenirRepository : ISouvenirRepository
    {
        private static class Constants
        {
            public const string DataPropertyName = "HolographicPhotoProjectData";
        }

        /// <summary>
        /// Key: userId, Value: souvenirModel
        /// 
        /// </summary>
        private static Dictionary<string, List<SouvenirModel>> existingSouvenirModel;

        private static IDataManager roamingDataManager;

        public SouvenirRepository(IDataManager dataManager)
        {
            roamingDataManager = dataManager;
            if (existingSouvenirModel == null)
            {
                existingSouvenirModel = new Dictionary<string, List<SouvenirModel>>();
            }
        }

        /// <summary>
        /// Loads the saved souvenirs into the list of existing souvenirs.
        /// </summary>
        public List<SouvenirModel> LoadSouvenirs(string userId)
        {
            existingSouvenirModel = roamingDataManager.LoadData<Dictionary<string, List<SouvenirModel>>>(Constants.DataPropertyName);

            if (existingSouvenirModel == null)
            {
                Debug.Log("The roaming data is empty. No souvenirs to get.");
                existingSouvenirModel = new Dictionary<string, List<SouvenirModel>>();
                return new List<SouvenirModel>();
            }
            if (!existingSouvenirModel.Keys.Contains(userId))
            {
                Debug.Log("The roaming data is empty for user with id " + userId);
                existingSouvenirModel.Add(userId, new List<SouvenirModel>());
            }

            // We return a deep copy of the user's souvenirs list so that modifying it does not affect the roaming data.
            List<SouvenirModel> souvenirListCopy = new List<SouvenirModel>();
            foreach (var souvenirModel in existingSouvenirModel[userId])
            {
                souvenirListCopy.Add(new SouvenirModel(souvenirModel));
            }
            
            return souvenirListCopy;
        }

        /// <summary>
        /// Add the souvenir data to the souvenir repository.
        /// </summary>
        public void AddSouvenir(string userId, SouvenirModel souvenirModel)
        {
            List<SouvenirModel> list;
            if (!existingSouvenirModel.TryGetValue(userId, out list))
            {
                list = new List<SouvenirModel>();
                existingSouvenirModel.Add(userId, list);
            }

            list.Add(new SouvenirModel(souvenirModel.Id, souvenirModel.TemplatePrefabPath, souvenirModel.BundleId));
            roamingDataManager.SaveData(Constants.DataPropertyName, existingSouvenirModel);
        }

        /// <summary>
        /// Removes souvenir from the repository.
        /// </summary>
        public void RemoveSouvenir(string userId, string dataId)
        {
            if (existingSouvenirModel[userId] == null)
            {
                Debug.LogWarning("The userId cannot be found.");
                return;
            }
            existingSouvenirModel[userId].RemoveAll((SouvenirModel data) => { return data.Id == dataId; });
            roamingDataManager.SaveData(Constants.DataPropertyName, existingSouvenirModel);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HolographicPhotoProject.Data.Onedrive.Controllers;
using HolographicPhotoProject.Presenters.Sources;
using HolographicPhotoProject.Data.RoamingProfiles;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;

#if UNITY_UWP
using HolographicPhotoProject.Utilities;
using Microsoft.Practices.Unity;
#endif

namespace HolographicPhotoProject.Souvenirs
{
    /// <summary>
    /// Component that allows the creation of souvenirs.
    /// </summary>
    public class SouvenirManager : MonoBehaviour
    {
        private static class Constants
        {
            public const string SouvenirPrefab = "Prefabs/Souvenir";
            public const string ModelsPrefabFolder = "Prefabs/Souvenirs/";
        }

        private WorldAnchorManager anchorManager;
        private static GameObject souvenirPrefab;
        private static Dictionary<string, GameObject> modelPrefabs = new Dictionary<string, GameObject>();
        private SpatialMappingObserver mappingObserver;
        private bool souvenirsPlaced;
        private List<SouvenirModel> unplacedSouvenirList = new List<SouvenirModel>();
        private string currentUser;

        public string LoggedInUser
        {
            get
            {
#if UNITY_UWP
                return OneDriveController.Instance.GetUserId();
#else
                return null;
#endif
            }
        }

        private void Start()
        {
            anchorManager = WorldAnchorManager.Instance;

            var spatialMappingManager = SpatialMappingManager.Instance;
            if (spatialMappingManager == null)
            {
                return;
            }

            mappingObserver = spatialMappingManager.Source as SpatialMappingObserver;
            if (mappingObserver != null)
            {
                mappingObserver.SurfaceAdded += SouvenirManager_OnMappingChanged;
            }
        }

        private void OnDestroy()
        {
            if (mappingObserver != null)
            {
                mappingObserver.SurfaceAdded -= SouvenirManager_OnMappingChanged;
            }
        }

        /// <summary>
        /// Loads the data from souvenir repository and creates souvenirs of the logged-in user.
        /// </summary>
        public void TryPlaceSouvenirs()
        {
            if (souvenirsPlaced && currentUser == LoggedInUser)
            {
                Debug.Log("Souvenirs are already loaded.");
                return;
            }

            if (string.IsNullOrEmpty(LoggedInUser))
            {
                Debug.LogWarning("User ID could not be retrieved.");
                return;
            }

            if (anchorManager == null)
            {
                Debug.LogWarning("World Anchor Manager could not be retrieved.");
                return;
            }

            if (anchorManager.AnchorStore == null)
            {
                Debug.LogWarning("Anchor Store could not be retrieved.");
                return;
            }

#if UNITY_UWP
            if (currentUser != LoggedInUser)
            {
                currentUser = LoggedInUser;

                // Load and cache the user's souvenirs to be placed.
                IUnityContainer container = UnityDIContainer.Get();
                var souvenirRepository = container.Resolve<SouvenirRepository>();
                unplacedSouvenirList = souvenirRepository.LoadSouvenirs(LoggedInUser);
            }
#endif

            souvenirsPlaced = true;

            for (int i = 0; i < unplacedSouvenirList.Count; ++i)
            {
                var souvenir = unplacedSouvenirList[i];
                if (WorldAnchorExists(souvenir.Id))
                {
                    CreateSouvenir(souvenir.Id, souvenir.TemplatePrefabPath, souvenir.BundleId, isNewSouvenir: false);

                    unplacedSouvenirList.RemoveAt(i);
                    --i;
                }
                else
                {
                    souvenirsPlaced = false;
                }
            }
        }

        /// <summary>
        /// Deletes the souvenirs of the user from the environment.
        /// </summary>
        public void TryUnloadSouvenirs()
        {
            Souvenir[] existingSouvenirs = FindObjectsOfType<Souvenir>();
            foreach (var souvenir in existingSouvenirs)
            {
                Destroy(souvenir.gameObject);
            }

            unplacedSouvenirList = null;
            currentUser = null;
            souvenirsPlaced = false;
        }

        /// <summary>
        /// Checks if the anchorId already exists in the Anchor store.
        /// </summary>
        private bool WorldAnchorExists(string id)
        {
            if (anchorManager == null)
            {
                Debug.LogError("WorldAnchorManager instance is null.");
                return false;
            }

            WorldAnchorStore anchorStore = anchorManager.AnchorStore;
            if (anchorStore == null)
            {
                Debug.LogError("Anchor store does not exist.");
                return false;
            }

            WorldAnchor existingAnchor = anchorStore.Load(id, new GameObject());
            if (existingAnchor == null)
            {
                Debug.LogError("Loading of the anchorId returned null.");
                return false;
            }

            return existingAnchor.isLocated;
        }

        /// <summary>
        /// Creates a Souvenir instance and binds the selected model to it.
        /// </summary>
        public void CreateSouvenir(string souvenirId, string prefabPath, string bundleId, bool isNewSouvenir = true)
        {
            if (souvenirPrefab == null)
            {
                souvenirPrefab = Resources.Load<GameObject>(Constants.SouvenirPrefab);
                if (souvenirPrefab == null)
                {
                    Debug.LogError("Could not load the Souvenir prefab.");
                    return;
                }
            }

            GameObject souvenirInstance = Instantiate(souvenirPrefab);
            if (souvenirInstance == null)
            {
                Debug.LogError("Could not instantiate the Souvenir prefab.");
                return;
            }
            souvenirInstance.SetActive(false);

            string modelPrefabPath = Constants.ModelsPrefabFolder + prefabPath;
            GameObject modelPrefab;

            bool isModelLoaded = modelPrefabs.TryGetValue(modelPrefabPath, out modelPrefab);

            if (!isModelLoaded || modelPrefab == null)
            {
                modelPrefab = Resources.Load<GameObject>(modelPrefabPath);
                if (modelPrefab == null)
                {
                    Debug.LogError("Could not load the selected model prefab.");
                    return;
                }

                modelPrefabs.Add(modelPrefabPath, modelPrefab);
            }

            GameObject modelInstance = Instantiate(modelPrefab);
            if (modelInstance == null)
            {
                Debug.LogError("Could not instantiate the selected model prefab.");
                return;
            }
            modelInstance.transform.parent = souvenirInstance.transform;

            var souvenirComponent = souvenirInstance.GetComponent<Souvenir>();
            if (souvenirComponent == null)
            {
                Debug.LogError("Could not find the HSSouvenir component in the souvenir instance.");
                return;
            }
            souvenirComponent.Id = souvenirId;

            var contentProvider = new OneDriveProvider(bundleId);
            souvenirComponent.ContentProvider = contentProvider;

            if (isNewSouvenir)
            {
                souvenirComponent.StartPlacing();
#if UNITY_UWP
                // Adds the data of the new souvenir in the roaming data.
                UnityContainer container = UnityDIContainer.Get();
                SouvenirRepository souvenirRepository = container.Resolve<SouvenirRepository>();
                souvenirRepository.AddSouvenir(LoggedInUser, new SouvenirModel(souvenirId, prefabPath, bundleId));
#endif
            }
            else
            {
                if (anchorManager.AnchorStore != null)
                {
                    anchorManager.AnchorStore.Load(souvenirId, souvenirInstance);
                }
                else
                {
                    Debug.Log("The anchor store was called before it was initialized.");
                }
            }

            var carouselPresenterSource = souvenirInstance.AddComponent<CarouselPresenterSource>();
            carouselPresenterSource.Souvenir = souvenirComponent;

            souvenirInstance.SetActive(true);
        }

        /// <summary>
        /// Tries to load the souvenirs when spatial mapping has changed.
        /// </summary>
        private void SouvenirManager_OnMappingChanged(object sender, DataEventArgs<SpatialMappingSource.SurfaceObject> e)
        {
            TryPlaceSouvenirs();
        }
    }
}
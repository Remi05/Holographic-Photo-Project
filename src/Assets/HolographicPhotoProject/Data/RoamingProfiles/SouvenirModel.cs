// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.Serialization;

namespace HolographicPhotoProject.Data.RoamingProfiles
{
    [DataContract]
    public class SouvenirModel
    {
        [DataMember(Name = "Id")]
        public string Id;

        [DataMember(Name = "TemplatePrefabPath")]
        public string TemplatePrefabPath;

        [DataMember(Name = "BundleId")]
        public string BundleId;

        public SouvenirModel() { }

        public SouvenirModel(string id, string prefabPath, string bundleId)
        {
            Id = id;
            TemplatePrefabPath = prefabPath;
            BundleId = bundleId;
        }

        public SouvenirModel(SouvenirModel toCopy)
        {
            Id = toCopy.Id;
            TemplatePrefabPath = toCopy.TemplatePrefabPath;
            BundleId = toCopy.BundleId;
        }
    }
}
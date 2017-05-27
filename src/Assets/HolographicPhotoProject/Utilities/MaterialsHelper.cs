// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.Utilities
{
    public class MaterialsHelper
    {
        /// <summary>
        /// Changes the material of the target to the given material.
        /// </summary>
        public static void ChangeMaterial(Transform target, Material material, int index = 0)
        {
            if (target == null || material == null)
            {
                Debug.LogError("Cannot change the material because the target or material could not be found.");
                return;
            }
            Renderer[] childrenRenderers = target.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in childrenRenderers)
            {
                if (renderer != null)
                {
                    int length = renderer.materials.Length;
                    if (index >= length && index < 0)
                    {
                        Debug.LogWarning("There material index cannot be found.");
                        return;
                    }
                    Material[] newMaterials = new Material[length];
                    for (var i = 0; i < length; ++i)
                    {
                        newMaterials[i] = renderer.materials[i];
                    }
                    newMaterials[index] = material;
                    renderer.materials = newMaterials;
                }
            }
        }
    }
}

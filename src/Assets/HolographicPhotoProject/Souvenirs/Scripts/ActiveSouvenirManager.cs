// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HolographicPhotoProject.Souvenirs
{
    /// <summary>
    /// Manages the Souvenir currently presenting content. 
    /// </summary>
    public class ActiveSouvenirManager : MonoBehaviour
    {
        private Souvenir currentSouvenir;

        /// <summary>
        /// Indicates whether the given Souvenir can be put in a passive state or not.
        /// </summary>
        private bool CanMakePassive(Souvenir souvenir)
        {
            return currentSouvenir == null || !currentSouvenir.IsInActiveState;
        }

        /// <summary>
        /// Checks if the given Souvenir is the currently active Souvenir.
        /// </summary>
        public bool IsActiveSouvenir(Souvenir souvenir)
        {
            return souvenir == currentSouvenir;
        }

        /// <summary>
        /// Checks if the given Souvenir can be put in a passive 
        /// state. If so, it becomes the active Souvenir and the
        /// previously active Souvenir is put in a sleeping state.
        /// </summary>
        public bool TryMakePassive(Souvenir souvenir)
        {
            if (CanMakePassive(souvenir))
            {
                if (currentSouvenir != null && currentSouvenir != souvenir)
                {
                    currentSouvenir.Sleep();
                }
                currentSouvenir = souvenir;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the given Souvenir as the currently active Souvenir
        /// and puts the previously active Souvenir in a sleeping state.
        /// </summary>
        public void MakeActive(Souvenir souvenir)
        {
            if (currentSouvenir == souvenir)
            {
                return;
            }

            if (currentSouvenir != null)
            {
                currentSouvenir.Sleep();
            }
            currentSouvenir = souvenir;
        }

        /// <summary>
        /// Unsets the given Souvenir as the active Souvenir if it currently is.
        /// </summary>
        public void MakeInactive(Souvenir souvenir)
        {
            if (currentSouvenir == souvenir)
            {
                currentSouvenir = null;
            }
        }
    }
}
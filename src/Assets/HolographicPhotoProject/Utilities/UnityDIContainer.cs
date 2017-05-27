// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_UWP
using Microsoft.Practices.Unity;
using HolographicPhotoProject.Data;
using HolographicPhotoProject.Data.RoamingProfiles;
using HolographicPhotoProject.Souvenirs;
using System;

namespace HolographicPhotoProject.Utilities
{
    /// <summary>
    /// Wrapper class for UnityContainer (Dependency Injection).
    /// </summary>
    public class UnityDIContainer : UnityContainer
    {
        private static readonly Lazy<UnityDIContainer> Lazy = new Lazy<UnityDIContainer>(() => 
                                                                new UnityDIContainer());

        /// <summary>
        /// Creates and maintains the same instance of the container in the app.
        /// </summary>
        public static UnityDIContainer Get()
        {
            if (!Lazy.IsValueCreated)
            {
                Lazy.Value.RegisterType<IDataManager, RoamingDataManager>(new ContainerControlledLifetimeManager());
                Lazy.Value.RegisterType<ISouvenirRepository, SouvenirRepository>(new ContainerControlledLifetimeManager());
            }
            return Lazy.Value;
        }

        private UnityDIContainer() { }
    }
}
#endif

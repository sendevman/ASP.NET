﻿namespace Abp.Modules
{
    public abstract class AbpModule : IAbpModule
    {
        /// <summary>
        /// What can be done in this method:
        /// - Make things those must be done before dependency registers.
        /// </summary>
        /// <param name="initializationContext"> </param>
        public virtual void PreInitialize(IAbpInitializationContext initializationContext)
        {

        }

        /// <summary>
        /// What can be done in this method:
        /// - Register dependency installers and components.
        /// </summary>
        /// <param name="initializationContext"> </param>
        public virtual void Initialize(IAbpInitializationContext initializationContext)
        {

        }

        /// <summary>
        /// What can be done in this method:
        /// - Make things those must be done after dependency registers.
        /// </summary>
        /// <param name="initializationContext"> </param>
        public virtual void PostInitialize(IAbpInitializationContext initializationContext)
        {
            
        }
    }
}

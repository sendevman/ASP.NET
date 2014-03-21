﻿using System;
using System.Reflection;
using Abp.Dependency;
using Abp.Modules;
using Abp.Startup;

namespace MySpaProject
{
    public class MySpaProjectApplicationModule : AbpModule
    {
        public override Type[] GetDependedModules()
        {
            return new[]
                   {
                       typeof(MySpaProjectCoreModule)
                   };
        }

        public override void Initialize(IAbpInitializationContext initializationContext)
        {
            base.Initialize(initializationContext);
            IocManager.Instance.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}

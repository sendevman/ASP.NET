using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Abp.Modules;
using Taskever.Web.App_Start.Dependency.Installers;

namespace Taskever.Web.App_Start    
{
    [AbpModule("Taskever.Web.Spa", Dependencies = new[] { "Abp.Web.Mvc", "Taskever.Web.Api" })]
    public class TaskeverWebSpaModule : AbpModule
    {
        public override void Initialize(IAbpInitializationContext initializationContext)
        {
            base.Initialize(initializationContext);

            AreaRegistration.RegisterAllAreas();
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            initializationContext.IocContainer.Install(new TaskeverWebSpaInstaller());
        }
    }
}
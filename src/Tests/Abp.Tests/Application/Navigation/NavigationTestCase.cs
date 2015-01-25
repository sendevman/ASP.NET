using System.Threading.Tasks;
using Abp.Application.Navigation;
using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Localization;
using NSubstitute;

namespace Abp.Tests.Application.Navigation
{
    internal class NavigationTestCase
    {
        public NavigationManager NavigationManager { get; private set; }

        public UserNavigationManager UserNavigationManager { get; private set; }

        private readonly IIocManager _iocManager;

        public NavigationTestCase()
            : this(new IocManager())
        {
        }

        public NavigationTestCase(IIocManager iocManager)
        {
            _iocManager = iocManager;
            Initialize();
        }

        private void Initialize()
        {
            //Navigation providers should be registered
            _iocManager.Register<MyNavigationProvider1>();
            _iocManager.Register<MyNavigationProvider2>();

            //Preparing navigation configuration
            var configuration = new NavigationConfiguration();
            configuration.Providers.Add<MyNavigationProvider1>();
            configuration.Providers.Add<MyNavigationProvider2>();

            //Initializing navigation manager
            NavigationManager = new NavigationManager(_iocManager, configuration);
            NavigationManager.Initialize();

            //Create user navigation manager to test
            UserNavigationManager = new UserNavigationManager(NavigationManager)
                                    {
                                        PermissionChecker = CreateMockPermissionChecker()
                                    };
        }

        private static IPermissionChecker CreateMockPermissionChecker()
        {
            var permissionManager = Substitute.For<IPermissionChecker>();
            permissionManager.IsGrantedAsync(1, "Abp.Zero.UserManagement").Returns(Task.FromResult(true));
            permissionManager.IsGrantedAsync(1, "Abp.Zero.RoleManagement").Returns(Task.FromResult(false));
            return permissionManager;
        }

        public class MyNavigationProvider1 : NavigationProvider
        {
            public override void SetNavigation(INavigationProviderContext context)
            {
                context.Manager.MainMenu.AddItem(
                    new MenuItemDefinition(
                        "Abp.Zero.Administration",
                        new FixedLocalizableString("Administration"),
                        "fa fa-asterisk",
                        requiresAuthentication: true
                        ).AddItem(
                            new MenuItemDefinition(
                                "Abp.Zero.Administration.User",
                                new FixedLocalizableString("User management"),
                                "fa fa-users",
                                "#/admin/users",
                                requiredPermissionName: "Abp.Zero.UserManagement"
                                )
                        ).AddItem(
                            new MenuItemDefinition(
                                "Abp.Zero.Administration.Role",
                                new FixedLocalizableString("Role management"),
                                "fa fa-star-o",
                                "#/admin/roles",
                                requiredPermissionName: "Abp.Zero.RoleManagement"
                                )
                        )
                    );
            }
        }

        public class MyNavigationProvider2 : NavigationProvider
        {
            public override void SetNavigation(INavigationProviderContext context)
            {
                var adminMenu = context.Manager.MainMenu.GetItemByName("Abp.Zero.Administration");
                adminMenu.AddItem(
                    new MenuItemDefinition(
                        "Abp.Zero.Administration.Setting",
                        new FixedLocalizableString("Setting management"),
                        icon: "fa fa-cog",
                        url: "#/admin/settings"
                        )
                    );
            }
        }
    }
}
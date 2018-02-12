using ConsulExec.Domain;

namespace ConsulExec.ViewModel
{
    public static class ProfileViewModelsFabric
    {
        public static ProfileViewModel<StartupOptions> Create(StartupOptions StartupOptions) =>
            new ProfileViewModel<StartupOptions>(StartupOptions, o => $"{o.Name} [{o.Connection?.Name}]");


        public static ProfileViewModel<ConnectionOptions> Create(ConnectionOptions ConnectionOptions) =>
            new ProfileViewModel<ConnectionOptions>(ConnectionOptions, o => o.Name);
    }
}

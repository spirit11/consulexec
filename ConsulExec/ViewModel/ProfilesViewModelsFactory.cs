using ConsulExec.Domain;

namespace ConsulExec.ViewModel
{
    public static class ProfilesViewModelsFactory
    {
        public static ProfileViewModel<StartupOptions> Create(StartupOptions StartupOptions) =>
            new ProfileViewModel<StartupOptions>(StartupOptions, o => o.Name);


        public static ProfileViewModel<ConnectionOptions> Create(ConnectionOptions ConnectionOptions) =>
            new ProfileViewModel<ConnectionOptions>(ConnectionOptions, o => o.Name);
    }
}

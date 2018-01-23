using ConsulExec.Domain;

namespace ConsulExec.ViewModel
{
    public static class StartupOptionsProfileViewModel
    {
        public static ProfileViewModel<StartupOptions> Create(StartupOptions StartupOptions) =>
            new ProfileViewModel<StartupOptions>(StartupOptions, o => o.Name);
    }
}

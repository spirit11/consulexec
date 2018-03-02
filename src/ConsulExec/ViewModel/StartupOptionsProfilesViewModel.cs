using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using StartupOptionsViewModel = ProfileViewModel<StartupOptions>;


    public delegate StartupOptions StartupOptionsFactoryDelegate(string Name);


    public class StartupOptionsProfilesViewModel : ProfilesViewModel<StartupOptionsViewModel>
    {
        public StartupOptionsProfilesViewModel(EditProfileDelegate EditProfile,
            UndoListViewModel UndoList,
            ConnectionProfilesViewModel ConnectionProfiles,
            ReactiveList<StartupOptionsViewModel> StartupProfiles,
            StartupOptionsFactoryDelegate StartupOptionsFactory = null)
            : base(EditProfile, UndoList, StartupProfiles)
        {
            this.ConnectionProfiles = ConnectionProfiles;
            startupOptionsFactory = StartupOptionsFactory ?? DefaultOptionsFabric;
        }

        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        protected override StartupOptionsViewModel CreateProfile(string NewName) =>
            ProfileViewModelsFactory.Create(startupOptionsFactory(NewName));

        protected override void Restore(StartupOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (SequentialStartupOptions)O;

        protected override object Backup(StartupOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();

        private static SequentialStartupOptions DefaultOptionsFabric(string NewName)
        {
            return new SequentialStartupOptions(new string[0]) { Name = NewName };
        }

        private readonly StartupOptionsFactoryDelegate startupOptionsFactory;
    }
}
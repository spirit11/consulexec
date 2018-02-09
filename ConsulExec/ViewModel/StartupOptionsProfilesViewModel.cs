using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using StartupOptionsViewModel = ProfileViewModel<StartupOptions>;


    public delegate StartupOptions StartupOptionsFabricDelegate(string Name);


    public class StartupOptionsProfilesViewModel : ProfilesViewModel<StartupOptionsViewModel>
    {
        public StartupOptionsProfilesViewModel(EditProfileDelegate EditProfile,
            UndoListViewModel UndoList,
            ConnectionProfilesViewModel ConnectionProfiles,
            ReactiveList<StartupOptionsViewModel> StartupProfiles,
            StartupOptionsFabricDelegate StartupOptionsFabric = null)
            : base(EditProfile, UndoList, StartupProfiles)
        {
            this.ConnectionProfiles = ConnectionProfiles;
            startupOptionsFabric = StartupOptionsFabric ?? DefaultOptionsFabric;
        }

        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        protected override StartupOptionsViewModel CreateProfile(string NewName) =>
            ProfilesViewModelsFactory.Create(startupOptionsFabric(NewName));

        protected override void Restore(StartupOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (SequentialStartupOptions)O;

        protected override object Backup(StartupOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();

        private static SequentialStartupOptions DefaultOptionsFabric(string NewName)
        {
            return new SequentialStartupOptions(new string[0]) { Name = NewName };
        }

        private readonly StartupOptionsFabricDelegate startupOptionsFabric;
    }
}
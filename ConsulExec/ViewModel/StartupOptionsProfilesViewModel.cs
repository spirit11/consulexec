using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using StartupOptionsViewModel = ProfileViewModel<StartupOptions>;


    public class StartupOptionsProfilesViewModel : ProfilesViewModel<StartupOptionsViewModel>
    {
        public delegate StartupOptions OptionsFabricDelegate(string Name);

        public StartupOptionsProfilesViewModel(EditProfileDelegate EditProfile,
            UndoListViewModel UndoList,
            ConnectionProfilesViewModel ConnectionProfiles,
            ReactiveList<StartupOptionsViewModel> StartupProfiles,
            OptionsFabricDelegate OptionsFabric = null)
            : base(EditProfile, UndoList, StartupProfiles)
        {
            this.ConnectionProfiles = ConnectionProfiles;
            optionsFabric = OptionsFabric ?? DefaultOptionsFabric;
        }

        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        protected override StartupOptionsViewModel CreateProfile(string NewName) =>
            ProfilesViewModelsFactory.Create(optionsFabric(NewName));

        protected override void Restore(StartupOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (SequentialStartupOptions)O;

        protected override object Backup(StartupOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();

        private static SequentialStartupOptions DefaultOptionsFabric(string NewName)
        {
            return new SequentialStartupOptions(new string[0]) { Name = NewName };
        }

        private readonly OptionsFabricDelegate optionsFabric;
    }
}
using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using StartupOptionsViewModel = ProfileViewModel<StartupOptions>;

    public class StartupOptionsProfilesViewModel : ProfilesViewModel<StartupOptionsViewModel>
    {
        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        public StartupOptionsProfilesViewModel(EditProfileDelegate EditProfile, UndoListViewModel UndoList,
            ConnectionProfilesViewModel ConnectionProfiles, ReactiveList<StartupOptionsViewModel> StartupProfiles)
            : base(EditProfile, UndoList, StartupProfiles)
        {
            this.ConnectionProfiles = ConnectionProfiles;
        }

        protected override StartupOptionsViewModel CreateProfile(string NewName) =>
            ProfilesViewModelsFactory.Create(new SequentialStartupOptions(new string[0]) { Name = NewName });

        protected override void Restore(StartupOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (SequentialStartupOptions)O;

        protected override object Backup(StartupOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();
    }
}
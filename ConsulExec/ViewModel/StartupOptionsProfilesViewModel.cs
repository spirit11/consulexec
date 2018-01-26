using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public class StartupOptionsProfilesViewModel : ProfilesViewModel<ProfileViewModel<StartupOptions>>
    {
        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        public StartupOptionsProfilesViewModel(EditProfileDelegate EditProfile, UndoListViewModel UndoList,
            ConnectionProfilesViewModel ConnectionProfiles, ReactiveList<ProfileViewModel<StartupOptions>> StartupProfiles)
            : base(EditProfile, UndoList, StartupProfiles)
        {
            this.ConnectionProfiles = ConnectionProfiles;
        }

        protected override ProfileViewModel<StartupOptions> CreateProfile(string NewName) =>
            ProfilesViewModelsFactory.Create(new SequentialStartupOptions(new string[0]) { Name = NewName });

        protected override void Restore(ProfileViewModel<StartupOptions> EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (SequentialStartupOptions)O;

        protected override object Backup(ProfileViewModel<StartupOptions> EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();
    }

    public class ConnectionProfilesViewModel : ProfilesViewModel<ProfileViewModel<ConnectionOptions>>
    {
        public ConnectionProfilesViewModel(EditProfileDelegate EditProfile, UndoListViewModel UndoList)
            : base(EditProfile, UndoList, new ReactiveList<ProfileViewModel<ConnectionOptions>>())
        {
        }

        protected override ProfileViewModel<ConnectionOptions> CreateProfile(string NewName) =>
            ProfilesViewModelsFactory.Create(new ConnectionOptions { Name = NewName });

        protected override void Restore(ProfileViewModel<ConnectionOptions> EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (ConnectionOptions)O;

        protected override object Backup(ProfileViewModel<ConnectionOptions> EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();
    }
}
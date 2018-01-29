using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using ConnectionOptionsViewModel = ProfileViewModel<ConnectionOptions>;

    public class ConnectionProfilesViewModel : ProfilesViewModel<ConnectionOptionsViewModel>
    {
        public ConnectionProfilesViewModel(EditProfileDelegate EditProfile, UndoListViewModel UndoList, ReactiveList<ConnectionOptionsViewModel> Profiles)
            : base(EditProfile, UndoList, Profiles)
        {
        }

        protected override ConnectionOptionsViewModel CreateProfile(string NewName) =>
            ProfilesViewModelsFactory.Create(new ConnectionOptions { Name = NewName });

        protected override void Restore(ConnectionOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (ConnectionOptions)O;

        protected override object Backup(ConnectionOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();
    }
}
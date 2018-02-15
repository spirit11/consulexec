using System.Collections.Generic;
using System.Linq;
using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using ConnectionOptionsViewModel = ProfileViewModel<ConnectionOptions>;


    public delegate ConnectionOptions ConnectionOptionsFactoryDelegate(string Name);


    public class ConnectionProfilesViewModel : ProfilesViewModel<ConnectionOptionsViewModel>
    {
        public ConnectionProfilesViewModel(EditProfileDelegate EditProfile,
            UndoListViewModel UndoList,
            ReactiveList<ConnectionOptionsViewModel> Profiles,
            ConnectionOptionsFactoryDelegate ConnectionOptionsFactory = null)
            : base(EditProfile, UndoList, Profiles)
        {
            connectionOptionsFactory = ConnectionOptionsFactory ?? (newName => new ConnectionOptions { Name = newName });
        }

        protected override ConnectionOptionsViewModel CreateProfile(string NewName) =>
            ProfileViewModelsFactory.Create(connectionOptionsFactory(NewName));

        protected override void Restore(ConnectionOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (ConnectionOptions)O;

        protected override object Backup(ConnectionOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();

        private readonly ConnectionOptionsFactoryDelegate connectionOptionsFactory;
    }

}
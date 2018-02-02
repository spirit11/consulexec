using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using ConnectionOptionsViewModel = ProfileViewModel<ConnectionOptions>;

    public class ConnectionProfilesViewModel : ProfilesViewModel<ConnectionOptionsViewModel>
    {
        public delegate ConnectionOptions OptionsFactoryDelegate(string Name);


        public ConnectionProfilesViewModel(EditProfileDelegate EditProfile, 
            UndoListViewModel UndoList,
            ReactiveList<ConnectionOptionsViewModel> Profiles,
            OptionsFactoryDelegate OptionsFactory = null)
            : base(EditProfile, UndoList, Profiles)
        {
            optionsFactory = OptionsFactory ?? (newName => new ConnectionOptions { Name = newName });
        }

        protected override ConnectionOptionsViewModel CreateProfile(string NewName) =>
            ProfilesViewModelsFactory.Create(optionsFactory(NewName));

        protected override void Restore(ConnectionOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (ConnectionOptions)O;

        protected override object Backup(ConnectionOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();

        private readonly OptionsFactoryDelegate optionsFactory;
    }
}
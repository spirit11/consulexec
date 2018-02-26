using System;
using System.Reactive.Linq;
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
            Func<ConnectionOptions, int> RequestUsages = null,
            ConnectionOptionsFactoryDelegate ConnectionOptionsFactory = null)
            : base(EditProfile, UndoList, Profiles)
        {
            connectionOptionsFactory = ConnectionOptionsFactory ?? (newName => new ConnectionOptions { Name = newName });
            var ru = RequestUsages ?? (v => -1);
            deleteTooltip = this.WhenAnyValue(v => v.Profile)
                .Select(profile => ru(profile?.Options).ToString()).ToProperty(this, vm => vm.DeleteTooltip);
        }

        public string DeleteTooltip => deleteTooltip.Value;
        private readonly ObservableAsPropertyHelper<string> deleteTooltip;

        protected override ConnectionOptionsViewModel CreateProfile(string NewName) =>
            ProfileViewModelsFactory.Create(connectionOptionsFactory(NewName));

        protected override void Restore(ConnectionOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (ConnectionOptions)O;

        protected override object Backup(ConnectionOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();

        private readonly ConnectionOptionsFactoryDelegate connectionOptionsFactory;
    }

}
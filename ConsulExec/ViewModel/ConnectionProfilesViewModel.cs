using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    using ConnectionOptionsViewModel = ProfileViewModel<ConnectionOptions>;


    public delegate ConnectionOptions ConnectionOptionsFactoryDelegate(string Name);


    public delegate int RequestUsageDelegate(ConnectionOptions Connection, StartupOptions IgnoredOwner);


    public class ConnectionProfilesViewModel : ProfilesViewModel<ConnectionOptionsViewModel>
    {
        public ConnectionProfilesViewModel(EditProfileDelegate EditProfile,
            UndoListViewModel UndoList,
            ReactiveList<ConnectionOptionsViewModel> Profiles,
            RequestUsageDelegate RequestUsage = null,
            ConnectionOptionsFactoryDelegate ConnectionOptionsFactory = null)
            : base(EditProfile, UndoList, Profiles)
        {
            connectionOptionsFactory = ConnectionOptionsFactory ?? (newName => new ConnectionOptions { Name = newName });
            var request = RequestUsage ?? ((v, o) => 0);
            var usages = this.WhenAnyValue(v => v.Profile)
                .CombineLatest(forceRequest, (profile, _) => request(profile?.Options, Owner));
            deleteTooltip = usages.Select(v => $"Used in {v} other settings.").ToProperty(this, vm => vm.DeleteTooltip);
            CanDelete = usages.Select(u => u == 0);
        }

        public string DeleteTooltip => deleteTooltip.Value;
        private readonly ObservableAsPropertyHelper<string> deleteTooltip;

        public override ConnectionOptionsViewModel Profile
        {
            get { return base.Profile; }
            set
            {
                base.Profile = value;
                forceRequest.OnNext(Unit.Default);
            }
        }

        public StartupOptions Owner { get; set; }

        protected override ConnectionOptionsViewModel CreateProfile(string NewName) =>
            ProfileViewModelsFactory.Create(connectionOptionsFactory(NewName));

        protected override void Restore(ConnectionOptionsViewModel EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (ConnectionOptions)O;

        protected override object Backup(ConnectionOptionsViewModel EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();

        private readonly ConnectionOptionsFactoryDelegate connectionOptionsFactory;
        private readonly BehaviorSubject<Unit> forceRequest = new BehaviorSubject<Unit>(Unit.Default);
    }
}
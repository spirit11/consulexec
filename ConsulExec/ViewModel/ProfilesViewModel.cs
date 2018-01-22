using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public class ProfilesViewModel : ReactiveObject
    {
        public ProfilesViewModel(CommandStartupSuccesorsFabric CommandStartupSuccesorsFabric)
        {
            var startupOptionsNotNull = this.WhenAnyValue(v => v.Profile).Select(v => v != null);

            DeleteCommand = ReactiveCommand.Create(() => RemoveProfile(Profile, true), startupOptionsNotNull);

            AddCommand = ReactiveCommand.Create(() =>
            {
                var name = "New " + List.Count;
                var newProfile = new ProfileViewModel(new SequentialStartupOptions(new string[0]) { Name = name });
                List.Add(newProfile);
                var undo = undoList.Push(() => RemoveProfile(newProfile, false));

                CommandStartupSuccesorsFabric.EditProfile(newProfile, vm => vm.HandlingCancel(op =>
                               {
                                   RemoveProfile(op, false);
                                   undo.Dispose();
                               })
                             .HandlingOk(op => Profile = op));
            });

            EditCommand = ReactiveCommand.Create(() =>
            {
                var editProfile = Profile;
                var backup = (SequentialStartupOptions)editProfile.Options.Clone();
                undoList.Push(() => editProfile.Options = backup);

                CommandStartupSuccesorsFabric.EditProfile(editProfile, vm => vm.HandlingDelete(_ => RemoveProfile(editProfile, true)));
            }, startupOptionsNotNull);
        }

        public ReactiveList<ProfileViewModel> List { get; } = new ReactiveList<ProfileViewModel>();

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand UndoCommand => undoList.UndoCommand;

        public ProfileViewModel Profile { get { return profile; } set { this.RaiseAndSetIfChanged(ref profile, value); } }
        private ProfileViewModel profile;

        private readonly UndoListViewModel undoList = new UndoListViewModel();

        private void RemoveProfile(ProfileViewModel RemovedOptions, bool AddToUndo)
        {
            if (AddToUndo)
            {
                var idx = List.IndexOf(RemovedOptions);
                var select = RemovedOptions == Profile;
                undoList.Push(() =>
                {
                    List.Insert(idx, RemovedOptions);
                    if (select)
                        Profile = RemovedOptions;
                });
            }
            if (Profile == RemovedOptions)
                Profile = List.FirstOrDefault(v => v != RemovedOptions);
            List.Remove(RemovedOptions);
        }
    }
}
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using ConsulExec.Domain;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    public abstract class ProfilesViewModel<T> : ReactiveObject
    {
        public delegate void EditProfileDelegate(T Profile, Action<IProfileEditorViewModel<T>> EditorSetup);

        public ReactiveList<T> List { get; } = new ReactiveList<T>();

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand UndoCommand => undoList.UndoCommand;

        public T Profile { get { return profile; } set { this.RaiseAndSetIfChanged(ref profile, value); } }
        private T profile;

        protected ProfilesViewModel(EditProfileDelegate EditProfile)
        {
            var startupOptionsNotNull = this.WhenAnyValue(v => v.Profile).Select(v => v != null);

            DeleteCommand = ReactiveCommand.Create(() => RemoveProfile(Profile, true), startupOptionsNotNull);

            AddCommand = ReactiveCommand.Create(() =>
            {
                var name = "New " + List.Count;
                var newProfile = CreateProfile(name);
                List.Add(newProfile);
                var undo = undoList.Push(() => RemoveProfile(newProfile, false));

                EditProfile(newProfile, vm => vm.HandlingCancel(op =>
                {
                    RemoveProfile(op, false);
                    undo.Dispose();
                }).HandlingOk(op => Profile = op));
            });

            EditCommand = ReactiveCommand.Create(() =>
            {
                var editProfile = Profile;
                var backup = Backup(editProfile);
                undoList.Push(() => Restore(editProfile, backup));

                EditProfile(editProfile, vm => vm.HandlingDelete(_ => RemoveProfile(editProfile, true)));
            }, startupOptionsNotNull);
        }

        protected abstract T CreateProfile(string NewName);

        protected abstract void Restore(T EditProfile, object O);

        protected abstract object Backup(T EditProfile);

        private readonly UndoListViewModel undoList = new UndoListViewModel();

        private void RemoveProfile(T RemovedOptions, bool AddToUndo)
        {
            if (AddToUndo)
            {
                var idx = List.IndexOf(RemovedOptions);
                var select = RemovedOptions.Equals(Profile);
                undoList.Push(() =>
                {
                    List.Insert(idx, RemovedOptions);
                    if (select)
                        Profile = RemovedOptions;
                });
            }
            if (RemovedOptions.Equals(Profile))
                Profile = List.FirstOrDefault(v => !v.Equals(RemovedOptions));
            List.Remove(RemovedOptions);
        }
    }


    public class ProfilesViewModel : ProfilesViewModel<ProfileViewModel<StartupOptions>>
    {
        public ProfilesViewModel(EditProfileDelegate EditProfile)
            : base(EditProfile)
        {
        }

        protected override ProfileViewModel<StartupOptions> CreateProfile(string NewName) =>
            StartupOptionsProfileViewModel.Create(new SequentialStartupOptions(new string[0]) { Name = NewName });

        protected override void Restore(ProfileViewModel<StartupOptions> EditStartupOptionsProfile, object O) =>
            EditStartupOptionsProfile.Options = (SequentialStartupOptions)O;

        protected override object Backup(ProfileViewModel<StartupOptions> EditStartupOptionsProfile) =>
            EditStartupOptionsProfile.Options.Clone();
    }
}
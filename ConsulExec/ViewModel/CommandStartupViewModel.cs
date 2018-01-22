using ReactiveUI;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ConsulExec.Domain;
using ConsulExec.Infrastructure;
using Splat;

namespace ConsulExec.ViewModel
{
    public class CommandStartupSuccesorsFabric
    {
        public CommandStartupSuccesorsFabric(Func<StartupOptions, string, CommandRunViewModel> RunCommand, IActivatingViewModel Activator)
        {
            runCommand = RunCommand;
            activator = Activator;
        }

        public CommandRunViewModel RunCommand(StartupOptions StartupOptions, string Command) =>
            runCommand(StartupOptions, Command);

        public ProfileEditorViewModel EditProfile(ProfileViewModel ProfileViewModel, Action<ProfileViewModel> DeleteCallback)
        {
            return new ProfileEditorViewModel(ProfileViewModel, activator,
                    Locator.Current.GetService<IRemoteExecution>().Nodes, DeleteCallback);
        }

        private readonly Func<StartupOptions, string, CommandRunViewModel> runCommand;
        private readonly IActivatingViewModel activator;
    }


    public class CommandStartupViewModel : ReactiveObject
    {
        public CommandStartupViewModel(CommandStartupSuccesorsFabric CommandStartupSuccesorsFabric, IActivatingViewModel Activator = null)
        {
            var startupOptionsNotNull = this.WhenAnyValue(v => v.Profile).Select(v => v != null);

#if DEBUG
            Command = "echo ok";
            if (!ModeDetector.InDesignMode())
            {
                RecentCommands.Add("echo ok");
                RecentCommands.Add("ping ya.ru");
            }

            Profiles.Add(new ProfileViewModel(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt" }));
            Profile = Profiles.First();
#endif

            ExecuteCommand = ReactiveCommand.Create(() =>
            {
                var cmd = Command;
                RecentCommands.Remove(cmd);
                RecentCommands.Add(cmd);
                Command = cmd;
                Activator?.Activate(CommandStartupSuccesorsFabric.RunCommand(Profile.Options, cmd));
            }, this.WhenAnyValue(v => v.Command, v => v.Profile, (cmd, opt) => !string.IsNullOrWhiteSpace(cmd) && opt != null));

            DeleteStartupOptionsCommand = ReactiveCommand.Create(() => RemoveOptions(Profile, true), startupOptionsNotNull);

            AddStartupOptionsCommand = ReactiveCommand.Create(() =>
            {
                string name = "New " + Profiles.Count;
                var newProfile = new ProfileViewModel(new SequentialStartupOptions(new string[0]) { Name = name });
                Profiles.Add(newProfile);
                var undo = undoList.Push(() => RemoveOptions(newProfile, false));

                Activator?.Activate(CommandStartupSuccesorsFabric.EditProfile(newProfile, op => { })
                    .HandlingCancel(op =>
                    {
                        RemoveOptions(op, false);
                        undo.Dispose();
                    })
                    .HandlingOk(op => Profile = op));
            });

            EditStartupOptionsCommand = ReactiveCommand.Create(() =>
            {
                var editProfile = Profile;
                var backup = (SequentialStartupOptions)editProfile.Options.Clone();
                undoList.Push(() => editProfile.Options = backup);

                Activator?.Activate(CommandStartupSuccesorsFabric.EditProfile(editProfile, op => RemoveOptions(editProfile, true)));
            }, startupOptionsNotNull);

            SetCommandCommand = ReactiveCommand.Create<string>(s => Command = s);
        }

        public string Command { get { return command; } set { this.RaiseAndSetIfChanged(ref command, value); } } // CommandViewModel
        private string command;

        public ReactiveList<string> RecentCommands { get; } = new ReactiveList<string>();

        public ProfileViewModel Profile { get { return profile; } set { this.RaiseAndSetIfChanged(ref profile, value); } }
        private ProfileViewModel profile;

        public ReactiveList<ProfileViewModel> Profiles { get; } = new ReactiveList<ProfileViewModel>();

        #region Commands

        public ICommand ExecuteCommand { get; }

        public ICommand AddStartupOptionsCommand { get; }

        public ICommand DeleteStartupOptionsCommand { get; }

        public ICommand EditStartupOptionsCommand { get; }

        public ICommand SetCommandCommand { get; }

        public ICommand UndoCommand => undoList.UndoCommand;

        #endregion

        private readonly UndoListViewModel undoList = new UndoListViewModel();

        private void RemoveOptions(ProfileViewModel RemovedOptions, bool AddToUndo)
        {
            if (AddToUndo)
            {
                var idx = Profiles.IndexOf(RemovedOptions);
                var select = RemovedOptions == Profile;
                undoList.Push(() =>
                {
                    Profiles.Insert(idx, RemovedOptions);
                    if (select)
                        Profile = RemovedOptions;
                });
            }
            if (Profile == RemovedOptions)
                Profile = Profiles.FirstOrDefault(v => v != RemovedOptions);
            Profiles.Remove(RemovedOptions);
        }
    }
}

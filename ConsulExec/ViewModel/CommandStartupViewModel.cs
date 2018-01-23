using ReactiveUI;
using System;
using System.Linq;
using System.Windows.Input;
using ConsulExec.Domain;
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

        public void EditProfile(ProfileViewModel<StartupOptions> StartupOptionsProfileViewModel, Action<ProfileEditorViewModel> SetupEditor)
        {
            var profileEditorViewModel = new ProfileEditorViewModel(StartupOptionsProfileViewModel, activator,
                Locator.Current.GetService<IRemoteExecution>().Nodes);
            SetupEditor(profileEditorViewModel);
            activator?.Activate(profileEditorViewModel);
        }

        private readonly Func<StartupOptions, string, CommandRunViewModel> runCommand;
        private readonly IActivatingViewModel activator;
    }


    public class CommandStartupViewModel : ReactiveObject
    {
        public CommandStartupViewModel(CommandStartupSuccesorsFabric CommandStartupSuccesorsFabric, IActivatingViewModel Activator = null)
        {
            Profiles = new ProfilesViewModel(CommandStartupSuccesorsFabric.EditProfile);

            ExecuteCommand = ReactiveCommand.Create(() =>
            {
                var cmd = Command;
                RecentCommands.Remove(cmd);
                RecentCommands.Add(cmd);
                Command = cmd;
                Activator?.Activate(CommandStartupSuccesorsFabric.RunCommand(Profiles.Profile.Options, cmd));
            }, this.WhenAnyValue(v => v.Command, v => v.Profiles.Profile, (cmd, opt) => !string.IsNullOrWhiteSpace(cmd) && opt != null));

            SetCommandCommand = ReactiveCommand.Create<string>(s => Command = s);

#if DEBUG
            Command = "echo ok";
            if (!ModeDetector.InDesignMode())
            {
                RecentCommands.Add("echo ok");
                RecentCommands.Add("ping ya.ru");
            }

            Profiles.List.Add(StartupOptionsProfileViewModel.Create(new SequentialStartupOptions(new[] { "Val-Pc2" }) { Name = "opt" }));
            Profiles.Profile = Profiles.List.First();
#endif
        }

        public string Command { get { return command; } set { this.RaiseAndSetIfChanged(ref command, value); } } // CommandViewModel
        private string command;

        public ReactiveList<string> RecentCommands { get; } = new ReactiveList<string>();

        public ProfilesViewModel Profiles { get; }

        #region Commands

        public ICommand ExecuteCommand { get; }

        public ICommand SetCommandCommand { get; }

        public ICommand UndoCommand => Profiles.UndoCommand;

        #endregion
    }
}

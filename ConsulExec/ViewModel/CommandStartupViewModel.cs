using ReactiveUI;
using System;
using System.Windows.Input;
using ConsulExec.Domain;
using Splat;

namespace ConsulExec.ViewModel
{
    public class EditorsFabric
    {
        public static ProfilesViewModel<ProfileViewModel<StartupOptions>>.EditProfileDelegate EditStartupOptions(IActivatingViewModel ActivatingViewModel,
            ConnectionProfilesViewModel Connections) =>
            (vm, setup) => EditStartupOptions(ActivatingViewModel, vm, setup, Connections);

        public static void EditStartupOptions(IActivatingViewModel ActivatingViewModel,
            ProfileViewModel<StartupOptions> StartupOptionsProfileViewModel,
            Action<StartupOptionsEditorViewModel> SetupEditor, ConnectionProfilesViewModel Connections)
        {
            var profileEditorViewModel = new StartupOptionsEditorViewModel(StartupOptionsProfileViewModel,
                Connections,
                ActivatingViewModel,
                Locator.Current.GetService<IRemoteExecution>().Nodes
                );
            SetupEditor(profileEditorViewModel);
            ActivatingViewModel?.Activate(profileEditorViewModel);
        }

        public static ProfilesViewModel<ProfileViewModel<ConnectionOptions>>.EditProfileDelegate EditConnectionOptions(IActivatingViewModel ActivatingViewModel)
            =>
            (vm, setup) =>
            {
                var profileEditorViewModel = new ConnectionOptionsEditorViewModel(new ConnectionOptions(), ActivatingViewModel);
                ActivatingViewModel?.Activate(profileEditorViewModel);
            };
    }


    public class CommandStartupViewModel : ReactiveObject
    {
        public CommandStartupViewModel(ConnectionProfilesViewModel ConnectionProfilesViewModel,
            StartupOptionsProfilesViewModel StartupOptionsProfilesViewModel,
            Action<StartupOptions, string> RunCommand)
        {
            ConnectionProfiles = ConnectionProfilesViewModel;
            StartupOptionsProfiles = StartupOptionsProfilesViewModel;

            ExecuteCommand = ReactiveCommand.Create(() =>
            {
                var cmd = Command;
                RecentCommands.Remove(cmd);
                RecentCommands.Add(cmd);
                Command = cmd;
                RunCommand?.Invoke(StartupOptionsProfiles.Profile.Options, cmd);
            }, this.WhenAnyValue(v => v.Command, v => v.StartupOptionsProfiles.Profile, (cmd, opt) => !string.IsNullOrWhiteSpace(cmd) && opt != null));

            SetCommandCommand = ReactiveCommand.Create<string>(s => Command = s);
        }

        public string Command { get { return command; } set { this.RaiseAndSetIfChanged(ref command, value); } } // CommandViewModel
        private string command;

        public ReactiveList<string> RecentCommands { get; } = new ReactiveList<string>();

        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        public StartupOptionsProfilesViewModel StartupOptionsProfiles { get; }

        #region Commands

        public ICommand ExecuteCommand { get; }

        public ICommand SetCommandCommand { get; }

        public ICommand UndoCommand => StartupOptionsProfiles.UndoCommand;

        #endregion
    }
}

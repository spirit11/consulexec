using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using ConsulExec.Domain;

namespace ConsulExec.ViewModel
{
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
                recentCommands.Remove(cmd);
                recentCommands.Add(cmd);

                if (recentCommands.Count > MaxRecentCommands)
                    recentCommands.RemoveRange(0, recentCommands.Count - MaxRecentCommands);

                Command = cmd;
                RunCommand?.Invoke(StartupOptionsProfiles.Profile.Options, cmd);
            }, this.WhenAnyValue(v => v.Command, v => v.StartupOptionsProfiles.Profile, (cmd, opt) => !string.IsNullOrWhiteSpace(cmd) && opt != null));

            SetCommandCommand = ReactiveCommand.Create<string>(s => Command = s);
        }

        public string Command { get { return command; } set { this.RaiseAndSetIfChanged(ref command, value); } } //TODO perhaps CommandViewModel
        private string command;

        public IReactiveDerivedList<string> RecentCommands => recentCommands.CreateDerivedCollection(v => v);
        private readonly ReactiveList<string> recentCommands = new ReactiveList<string>();

        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        public StartupOptionsProfilesViewModel StartupOptionsProfiles { get; }

        public void ClearRecentCommands() => recentCommands.Clear();

        public void AddRecentCommands(IEnumerable<string> Cmds) => recentCommands.AddRange(Cmds);

        #region Commands

        public ICommand ExecuteCommand { get; }

        public ICommand SetCommandCommand { get; }

        public ICommand UndoCommand => StartupOptionsProfiles.UndoCommand;

        #endregion

        private const int MaxRecentCommands = 20;
    }
}

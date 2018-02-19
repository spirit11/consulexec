using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ConsulExec.Domain;

namespace ConsulExec.ViewModel
{
    public class CommandStartupViewModel : ReactiveObject
    {
        public CommandStartupViewModel(ConnectionProfilesViewModel ConnectionProfilesViewModel,
            StartupOptionsProfilesViewModel StartupOptionsProfilesViewModel,
            Action<StartupOptions, string> RunCommand,
            ReactiveList<CommandViewModel> RecentCommands = null)
        {
            ConnectionProfiles = ConnectionProfilesViewModel;
            recentCommands = RecentCommands ?? new ReactiveList<CommandViewModel>();
            StartupOptionsProfiles = StartupOptionsProfilesViewModel;

            ExecuteCommand = ReactiveCommand.Create(() =>
            {
                var cmd = command;
                recentCommands.Remove(cmd);
                recentCommands.Add(cmd);

                if (recentCommands.Count > MaxRecentCommands)
                    recentCommands.RemoveRange(0, recentCommands.Count - MaxRecentCommands);

                Command = command.Command;
                RunCommand?.Invoke(StartupOptionsProfiles.Profile.Options, cmd.Command);
            }, this.WhenAnyValue(v => v.Command, v => v.StartupOptionsProfiles.Profile, (cmd, opt) => !string.IsNullOrWhiteSpace(cmd) && opt != null));

            SetCommandCommand = ReactiveCommand.Create<string>(s => Command = s);
        }

        public string Command
        {
            get
            {
                return command.Command;
            }
            set
            {
                var vm = recentCommands.FirstOrDefault(c => c.Command == value) ?? new CommandViewModel(value);
                this.RaiseAndSetIfChanged(ref command, vm);
            }
        }
        private CommandViewModel command = CommandViewModel.Empty;

        public IReactiveDerivedList<string> RecentCommands => recentCommands.CreateDerivedCollection(v => v.Command);
        private readonly ReactiveList<CommandViewModel> recentCommands;

        public ConnectionProfilesViewModel ConnectionProfiles { get; }

        public StartupOptionsProfilesViewModel StartupOptionsProfiles { get; }

        public void ClearRecentCommands() => recentCommands.Clear();

        public void AddRecentCommands(IEnumerable<string> Cmds) => recentCommands.AddRange(Cmds.Select(c => new CommandViewModel(c)));

        #region Commands

        public ICommand ExecuteCommand { get; }

        public ICommand SetCommandCommand { get; }

        public ICommand UndoCommand => StartupOptionsProfiles.UndoCommand;

        #endregion

        private const int MaxRecentCommands = 20;
    }
}

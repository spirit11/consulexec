using System.Diagnostics;
using ReactiveUI;

namespace ConsulExec.ViewModel
{
    [DebuggerDisplay("Command {" + nameof(Command) + "}")]
    public class CommandViewModel : ReactiveObject
    {
        public CommandViewModel(string Command)
        {
            this.Command = Command;
        }
        public string Command { get; }

        public static CommandViewModel Empty => new CommandViewModel("");
    }
}
namespace ConsulExec.ViewModel
{
    public class CommandViewModel
    {
        public string Command { get; set; }
        public static CommandViewModel Empty => new CommandViewModel();
    }
}
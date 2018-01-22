namespace ConsulExec.Domain
{
    public class NodeExecutionTask
    {
        public NodeExecutionTask(string Command, string NamePattern = null, string ServicePattern = null, string TagPattern = null)
        {
            this.NamePattern = NamePattern;
            this.ServicePattern = ServicePattern;
            this.TagPattern = TagPattern;
            this.Command = Command;
        }

        public readonly string NamePattern;
        public readonly string ServicePattern;
        public readonly string TagPattern;
        public readonly string Command;
    }
}
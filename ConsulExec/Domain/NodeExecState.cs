namespace ConsulExec.Domain
{
    enum NodeExecState
    {
        Idle,
        EventFired,
        Ack,
        Heartbeat,
        Unknown,
        Timeout,
        Done
    }
}
namespace PulseBridge.Worker;

public sealed class UnrecoverableMessageException : Exception
{
    public UnrecoverableMessageException(string message) : base(message) { }
    public UnrecoverableMessageException(string message, Exception inner) : base(message, inner) { }
}

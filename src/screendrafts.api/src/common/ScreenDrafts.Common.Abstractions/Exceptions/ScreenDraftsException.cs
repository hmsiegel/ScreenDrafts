namespace ScreenDrafts.Common.Abstractions.Exceptions;

public sealed class ScreenDraftsException : Exception
{
    public ScreenDraftsException(string requestName, SDError? error, Exception? innerException)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        SDError = error;
    }

    public ScreenDraftsException(string requestName, SDError? error)
        : this(requestName, error, null)
    {
    }

    public ScreenDraftsException(string requestName)
        : this(requestName, null, null)
    {
    }

    public ScreenDraftsException()
    {
    }

    public ScreenDraftsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public string? RequestName { get; }

    public SDError? SDError { get; }

}

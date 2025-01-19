namespace ScreenDrafts.Common.Application.Exceptions;

public sealed class ScreenDraftsException : Exception
{
    public ScreenDraftsException(string requestName, Error? error, Exception? innerException)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public ScreenDraftsException(string requestName, Error? error)
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

    public Error? Error { get; }

}

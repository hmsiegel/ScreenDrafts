namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static class HostErrors
{
  public static readonly SDError InvalidHostName =
    SDError.Failure(
      "HostError.InvalidHostName",
      "Host name cannot be empty or whitespace.");

  public static SDError NotFound(Guid hostId) => 
    SDError.NotFound(
      "HostError.NotFound",
      $"Host with id {hostId} was not found.");

  public static readonly SDError CannotCreateHost =
    SDError.Failure(
      "HostError.CannotCreateHost",
      "Cannot create host.");
}

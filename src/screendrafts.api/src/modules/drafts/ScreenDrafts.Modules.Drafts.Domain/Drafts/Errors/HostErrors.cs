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

  public static readonly SDError DraftCannotBeNull =
    SDError.Failure(
      "HostError.DraftCannotBeNull",
      "Draft cannot be null.");

  public static readonly SDError UserIdCannotBeEmpty =
    SDError.Failure(
      "HostError.UserIdCannotBeEmpty",
      "User ID cannot be empty.");

  public static SDError AlreadyExists(string personPublicId) =>
    SDError.Failure(
      "HostError.AlreadyExists",
      $"Host with person ID {personPublicId} already exists.");
}

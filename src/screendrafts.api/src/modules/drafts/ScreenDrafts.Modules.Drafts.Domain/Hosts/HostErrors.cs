using ScreenDrafts.Common.Abstractions.Errors;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScreenDrafts.Modules.Drafts.Domain.Hosts;

public static class HostErrors
{
  public static readonly SDError InvalidHostName = SDError.Failure(
    "HostError.InvalidHostName",
    "Host name cannot be empty or whitespace."
  );

  public static SDError NotFound(Guid hostId) =>
    SDError.NotFound("HostError.NotFound", $"Host with id {hostId} was not found.");

  public static SDError NotFound(string hostPublicId) =>
    SDError.NotFound("HostError.NotFound", $"Host with public ID {hostPublicId} was not found.");

  public static readonly SDError CannotCreateHost = SDError.Failure(
    "HostError.CannotCreateHost",
    "Cannot create host."
  );

  public static readonly SDError DraftCannotBeNull = SDError.Failure(
    "HostError.DraftCannotBeNull",
    "Draft cannot be null."
  );

  public static readonly SDError UserIdCannotBeEmpty = SDError.Failure(
    "HostError.UserIdCannotBeEmpty",
    "User ID cannot be empty."
  );

  public static readonly SDError AlreadyRetired = SDError.Failure(
    "HostError.AlreadyRetired",
    "Host is already retired."
  );

  public static SDError AlreadyExists(string personPublicId) =>
    SDError.Failure(
      "HostError.AlreadyExists",
      $"Host with person ID {personPublicId} already exists."
    );

  public static SDError NotFoundForPerson(string personPublicId) =>
  SDError.NotFound(
    "Host.NotFoundForPerson",
    $"No Host is linked to person '{personPublicId}'.");
}

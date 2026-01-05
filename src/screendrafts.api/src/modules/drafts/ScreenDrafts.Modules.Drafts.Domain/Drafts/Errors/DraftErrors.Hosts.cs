namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static partial class DraftErrors
{
  public static SDError HostAlreadyAdded(Guid hostId) =>
    SDError.Conflict(
      "Drafts.HostAlreadyTaken",
      $"Host with id {hostId} is already added to the draft.");

  public static readonly SDError TooManyHosts =
    SDError.Conflict(
      "Drafts.TooManyHosts",
      "Cannot add more hosts than the total allowed.");

  public static SDError PrimaryHostAlreadySet(Guid id) =>
    SDError.Failure(
      "Drafts.PrimaryHostAlreadySet",
      $"Primary host with id {id} is already set for the draft.");

  public static SDError InvalidHostRole(string role) =>
    SDError.Problem(
      "Drafts.InvalidHostRole",
      $"Invalid host role: {role}. Valid roles are 'Primary' and 'CoHost'.");
}

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static class HostErrors
{
  public static SDError NotFound(Guid hostId) => 
    SDError.NotFound(
      "HostError.NotFound",
      $"Host with id {hostId} was not found.");
}

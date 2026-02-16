namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetDrafter;

public sealed record DrafterResponse(
  Guid Id,
  Guid PersonId,
  string FirstName,
  string LastName,
  string DisplayName)
{
  public DrafterResponse()
      : this(
          Guid.Empty,
          Guid.Empty,
          string.Empty,
          string.Empty,
          string.Empty)
  {
  }
}

namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListDrafts;

public sealed record DrafterDraftResponse(
  Guid Id,
  Guid PersonId,
  string DisplayName)
{
  public DrafterDraftResponse()
    : this(
        Guid.Empty,
        Guid.Empty,
        string.Empty)
  {
  }
}

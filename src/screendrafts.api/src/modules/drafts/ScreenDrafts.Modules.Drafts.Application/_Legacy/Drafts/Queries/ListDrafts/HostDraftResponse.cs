namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.ListDrafts;

public sealed record HostDraftResponse(
  Guid Id,
  Guid PersonId,
  string DisplayName)
{
  public HostDraftResponse()
    : this(
        Guid.Empty,
        Guid.Empty,
        string.Empty)
  {
  }
}

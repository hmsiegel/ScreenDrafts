namespace ScreenDrafts.Modules.Drafts.Contracts;

public sealed record DraftResponse(
  Ulid Id,
  string Title,
  string DraftType,
  int TotalPicks,
  int TotalDrafters,
  int TotalHosts,
  string DraftStatus)
{
  public DraftResponse() : this(
    Ulid.Empty,
    string.Empty,
    string.Empty,
    default,
    default,
    default,
    string.Empty)
  {
  }
}

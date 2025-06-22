namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;

public sealed record UpcomingDraftDto
{
  public Guid Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public int DraftStatus { get; init; }
  public DateTime[] ReleaseDates { get; init; } = [];

  public required DraftUserCapabilities Capabilities { get; set; } = new(null, false, false, false, false);

  public void SetCapabilities(
    string? role,
    bool canEdit,
    bool canDelete,
    bool canStart,
    bool canPlay)
  {
    Capabilities = new DraftUserCapabilities(role, canEdit, canDelete, canStart, canPlay);
  }
}

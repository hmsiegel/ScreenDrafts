namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;

public sealed record UpcomingDraftDto
{
  public Guid Id { get; init; }
  public string Title { get; init; } = string.Empty;
  public int DraftStatus { get; init; }
  public DateTime[] ReleaseDates { get; init; } = [];
}

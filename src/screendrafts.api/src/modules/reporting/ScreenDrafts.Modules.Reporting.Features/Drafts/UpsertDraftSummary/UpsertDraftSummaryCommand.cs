namespace ScreenDrafts.Modules.Reporting.Features.Drafts.UpsertDraftSummary;

internal sealed record UpsertDraftSummaryCommand : ICommand
{
  public Guid DraftId { get; init; }
  public required string DraftPublicId { get; init; }
  public required string DraftPartPublicId { get; init; }
  public required string Title { get; init; }
  public required string DraftType { get; init; }
  public int PartIndex { get; init; }
  public int TotalParts { get; init; }
  public int TotalPicks { get; init; }
  public bool IsPatreon { get; init; }
  public int? EpisodeNumber { get; init; }
  public int VetoCount { get; init; }
}

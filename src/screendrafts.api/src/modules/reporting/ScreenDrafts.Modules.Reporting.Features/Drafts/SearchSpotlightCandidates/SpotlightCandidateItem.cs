namespace ScreenDrafts.Modules.Reporting.Features.Drafts.SearchSpotlightCandidates;

internal sealed record SpotlightCandidateItem
{
  public required string DraftPublicId { get; init; }
  public required string Title { get; init; }
  public required string DraftType { get; init; }
  public required int? EpisodeNumber { get; init; }
  public required int TotalPicks { get; init; }
}

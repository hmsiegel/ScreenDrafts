namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed record AddCandidateEntryRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  public required int TmdbId { get; init; }
  public string? Notes { get; init; }
  public required MediaType MediaType { get; init; }
  public int? TvSeriesTmdbId { get; init; }
  public int? SeasonNumber { get; init; }
  public int? EpisodeNumber { get; init; }
}

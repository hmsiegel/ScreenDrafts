namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.RemoveCandidateListEntry;

internal sealed record RemoveCandidateListEntryRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromRoute(Name = "tmdbId")]
  public required int TmdbId { get; init; }
}

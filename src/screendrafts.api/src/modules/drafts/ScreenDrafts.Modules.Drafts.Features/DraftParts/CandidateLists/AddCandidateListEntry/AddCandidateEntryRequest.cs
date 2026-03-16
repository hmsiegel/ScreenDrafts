namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed record AddCandidateEntryRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  public required int TmdbId { get; init; }
  public string? Notes { get; init; }
}

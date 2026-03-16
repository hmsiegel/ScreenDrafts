namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed record BulkAddCandidateEntriesRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPart { get; init; }

  /// <summary>
  /// CSV file with columns: Title, TmdbId
  /// Title is information only - TmdbId is the authoritative identifier.
  /// </summary>
  public required IFormFile File { get; init; }
}

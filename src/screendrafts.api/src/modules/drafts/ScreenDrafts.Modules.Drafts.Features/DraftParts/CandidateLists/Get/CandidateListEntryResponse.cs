namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.Get;

internal sealed record CandidateListEntryResponse
{
  public required Guid EntryId { get; init; }
  public required int TmdbId { get; init; }
  public string? MovieTitle { get; init; }
  public string? MovieImdbId { get; init; }
  public required string AddedByPublicId { get; init; }
  public string? Notes { get; init; }
  public required DateTime CreatedOnUtc { get; init; }

  public required bool IsPending { get; init; }
}

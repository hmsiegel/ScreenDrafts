namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed record BulkAddCandidateEntriesResponse
{
  public required int TotalRows { get; init; }
  public required int AddedEntries { get; init; }
  public required int SkipedEntries { get; init; }
  public required int FailedEntries { get; init; }
  public IReadOnlyList<BulkAddFailureDetail> Failures { get; init; } = [];
}

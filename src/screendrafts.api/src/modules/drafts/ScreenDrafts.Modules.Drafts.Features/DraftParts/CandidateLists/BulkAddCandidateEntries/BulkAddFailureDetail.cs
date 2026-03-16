namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed record BulkAddFailureDetail
{
  public required int RowNumber { get; init; }
  public required string? Title { get; init; }
  public required string Reason { get; init; }
}

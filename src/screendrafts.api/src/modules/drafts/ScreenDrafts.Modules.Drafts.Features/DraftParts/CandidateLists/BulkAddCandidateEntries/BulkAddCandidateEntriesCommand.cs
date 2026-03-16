namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed record BulkAddCandidateEntriesCommand : ICommand<BulkAddCandidateEntriesResponse>
{
  public required string DraftPartId { get; init; }
  public required Stream CsvStream { get; init; }
  public required string AddedByPublicId { get; init; }
}

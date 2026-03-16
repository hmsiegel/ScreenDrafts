namespace ScreenDrafts.Modules.Drafts.Features.Common.BulkAdd;

internal sealed record BulkAddMoviesResponse
{
  public required int TotalRows { get; init; }
  public required int AddedEntries { get; init; }
  public required int SkipedEntries { get; init; }
  public required int FailedEntries { get; init; }
  public IReadOnlyList<BulkAddFailureDetail> Failures { get; init; } = [];
}

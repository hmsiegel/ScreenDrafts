namespace ScreenDrafts.Modules.Drafts.Features.Common.BulkAdd;

internal sealed record BulkAddFailureDetail
{
  public required int RowNumber { get; init; }
  public required string? Title { get; init; }
  public required string Reason { get; init; }
}

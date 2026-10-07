namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record RecordBookTotal
{
  public required string Code { get; init; }
  public required string Label { get; init; }
  public required int Value { get; init; }
}

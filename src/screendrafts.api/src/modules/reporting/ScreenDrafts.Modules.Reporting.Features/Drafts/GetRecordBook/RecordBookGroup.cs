namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record RecordBookGroup
{
  public required string Key { get; init; }
  public required string Title { get; init; }
  public IReadOnlyList<RecordItem> Records { get; init; } = [];
}

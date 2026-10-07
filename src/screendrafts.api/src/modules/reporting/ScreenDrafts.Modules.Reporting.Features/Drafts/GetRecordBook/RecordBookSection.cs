namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record RecordBookSection
{
  public required string Key { get; init; }
  public required string Title { get; init; }
  public IReadOnlyList<RecordBookGroup> Groups { get; init; } = [];
}

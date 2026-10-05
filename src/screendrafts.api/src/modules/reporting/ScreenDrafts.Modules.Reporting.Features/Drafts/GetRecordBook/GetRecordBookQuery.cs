namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed record GetRecordBookQuery : IQuery<GetRecordBookResponse>
{
  public required bool IncludeAll { get; init; }
}

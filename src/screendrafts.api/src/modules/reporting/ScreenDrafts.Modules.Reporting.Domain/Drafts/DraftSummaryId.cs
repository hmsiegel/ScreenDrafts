namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public sealed record DraftSummaryId(Guid Value)
{
  public static DraftSummaryId CreateUnique() => new(Guid.NewGuid());

  public static DraftSummaryId Create(Guid value) => new(value);
}

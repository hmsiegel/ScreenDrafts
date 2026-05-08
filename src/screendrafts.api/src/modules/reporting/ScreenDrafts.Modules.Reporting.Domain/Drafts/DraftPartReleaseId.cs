namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public sealed record DraftPartReleaseId(Guid Value)
{
  public static DraftPartReleaseId CreateUnique() => new(Guid.NewGuid());

  public static DraftPartReleaseId Create(Guid id) => new(id);
}

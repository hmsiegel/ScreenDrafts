namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public sealed record DraftSpotlightId(Guid Value)
{
  public static DraftSpotlightId CreateUnique() => new(Guid.NewGuid());

  public static DraftSpotlightId Create(Guid value) => new(value);
}

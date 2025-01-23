namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record DrafterDraftStatsId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static DrafterDraftStatsId CreateUnique() => new(Guid.NewGuid());

  public static DrafterDraftStatsId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static DrafterDraftStatsId Create(Guid value) => new(value);
}

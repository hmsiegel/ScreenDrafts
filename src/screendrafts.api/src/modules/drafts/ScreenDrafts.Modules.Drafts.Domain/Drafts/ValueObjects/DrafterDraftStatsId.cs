namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

public sealed record DrafterDraftStatsId(Ulid Value)
{
  public Ulid Value { get; init; } = Value;

  public static DrafterDraftStatsId CreateUnique() => new(Ulid.NewUlid());

  public static DrafterDraftStatsId FromString(string value) => new(Ulid.Parse(value, CultureInfo.InvariantCulture));

  public static DrafterDraftStatsId Create(Ulid value) => new(value);
}

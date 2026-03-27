namespace ScreenDrafts.Modules.Reporting.Domain.Drafters;

public sealed record DrafterCanonicalAppearanceId(Guid Value)
{
  public static DrafterCanonicalAppearanceId Create(Guid value) => new(value);
  public static DrafterCanonicalAppearanceId CreateUnique() => new(Guid.NewGuid());
}

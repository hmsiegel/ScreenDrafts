using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record VetoId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static VetoId CreateUnique() => new(Guid.NewGuid());

  public static VetoId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static VetoId Create(Guid value) => new(value);
}

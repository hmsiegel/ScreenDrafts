using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record PickId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static PickId CreateUnique() => new(Guid.NewGuid());

  public static PickId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static PickId Create(Guid value) => new(value);
}

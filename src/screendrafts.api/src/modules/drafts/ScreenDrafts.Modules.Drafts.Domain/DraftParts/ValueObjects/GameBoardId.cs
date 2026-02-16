using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record GameBoardId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GameBoardId CreateUnique() => new(Guid.NewGuid());

  public static GameBoardId FromString(string value) => new(Guid.Parse(value, CultureInfo.InvariantCulture));

  public static GameBoardId Create(Guid value) => new(value);
}

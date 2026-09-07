namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.ValueObjects;

public sealed record GuestDraftGameBoardId(Guid Value)
{
  public Guid Value { get; init; } = Value;

  public static GuestDraftGameBoardId CreateUnique() => new(Guid.NewGuid());

  public static GuestDraftGameBoardId FromString(string value) => new(Guid.Parse(value));

  public static GuestDraftGameBoardId Create(Guid value) => new(value);
}

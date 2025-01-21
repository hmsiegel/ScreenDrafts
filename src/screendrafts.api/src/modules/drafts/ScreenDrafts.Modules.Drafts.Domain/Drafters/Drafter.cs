namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Drafter : AggrgateRoot<DrafterId, Ulid>
{
  private Drafter(
    DrafterId id,
    Ulid userId,
    string name)
    : base(id)
  {
    UserId = userId;
    Name = name;
  }

  private Drafter()
  {
  }

  public int ReadableId { get; init; }

  public Ulid UserId { get; private set; }

  public string Name { get; private set; } = default!;

  public static Drafter Create(
    string name,
    Ulid userId,
    DrafterId? id = null)
  {
    Guard.Against.Null(id);
    Guard.Against.Null(name);

    var drafter = new Drafter(
      id: id ?? DrafterId.CreateUnique(),
      userId: userId,
      name: name);

    drafter.Raise(new DrafterCreatedDomainEvent(drafter.Id.Value));

    return drafter;
  }
}

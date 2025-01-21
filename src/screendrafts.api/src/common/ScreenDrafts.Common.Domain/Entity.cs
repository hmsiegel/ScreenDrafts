namespace ScreenDrafts.Common.Domain;

public abstract class Entity
{
  protected Entity(Ulid id)
  {
    Id = id;
  }

  protected Entity()
  {
  }

  public Ulid Id { get; init; } = default!;
}

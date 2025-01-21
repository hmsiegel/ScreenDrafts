namespace ScreenDrafts.Common.Domain;

public abstract class Entity<TEntityId>
  where TEntityId : class
{
  protected Entity(TEntityId id)
  {
    Id = id;
  }

  protected Entity()
  {
  }

  public TEntityId Id { get; init; } = default!;
}

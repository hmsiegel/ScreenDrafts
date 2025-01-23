namespace ScreenDrafts.Common.Domain;

public abstract class Entity<TEntityId> : Entity
  where TEntityId : class
{
  protected Entity(TEntityId id)
  {
    Id = id;
  }

  protected Entity()
  {
  }

  public new TEntityId Id { get; init; } = default!;
}

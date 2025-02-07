using Bogus;

using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Common.UnitTests;

public abstract class BaseTest
{
  protected static readonly Faker Faker = new();

  public static T AssertDomainEventWasPublished<T>(Entity entity)
  {
    ArgumentNullException.ThrowIfNull(entity);

    var domainEvent = entity.DomainEvents.OfType<T>().SingleOrDefault();

    return domainEvent is null
      ? throw new InvalidOperationException($"Domain event of type {typeof(T).Name} was not published.")
      : domainEvent;
  }
}

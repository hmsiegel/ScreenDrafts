namespace ScreenDrafts.Modules.Drafts.Domain.Abstractions;

public interface IDomainEvent
{
  Guid Id { get; }

  DateTime OccurredOnUtc { get; }
}

namespace ScreenDrafts.Common.Features.Abstractions.EventBus;

public interface IIntegrationEvent
{
  Guid Id { get; }

  DateTime OccurredOnUtc { get; }
}

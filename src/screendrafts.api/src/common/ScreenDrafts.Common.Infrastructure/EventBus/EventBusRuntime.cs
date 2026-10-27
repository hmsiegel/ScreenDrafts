namespace ScreenDrafts.Common.Infrastructure.EventBus;

/// <summary>Holds the publish-only bus once <see cref="EventBusHostedService"/> has started it.</summary>
internal sealed class EventBusRuntime
{
  private IBus? _publisher;

  public IBus Publisher =>
    _publisher ?? throw new InvalidOperationException("The event bus has not started yet.");

  public void SetPublisher(IBus publisher) => _publisher = publisher;
}

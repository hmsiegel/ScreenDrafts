using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

/// <summary>
/// Replaces the base factory's <c>NoOpEventBus</c> in scenario tests.
/// Every integration event published via <see cref="IEventBus.PublishAsync{T}"/> is captured
/// in <see cref="CapturedEvents"/> instead of being silently discarded, so that
/// <see cref="Helpers.DraftScenarioBase.DispatchIntegrationEventsAsync"/> can deliver them
/// in-process to Communications and RealTimeUpdates consumers.
/// </summary>
public sealed class CapturingEventBus : IEventBus
{
  private readonly List<IIntegrationEvent> _captured = [];

  public IReadOnlyList<IIntegrationEvent> CapturedEvents => _captured.AsReadOnly();

  public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
    where T : IIntegrationEvent
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);
    _captured.Add(integrationEvent);
    return Task.CompletedTask;
  }

  public void Clear() => _captured.Clear();
}

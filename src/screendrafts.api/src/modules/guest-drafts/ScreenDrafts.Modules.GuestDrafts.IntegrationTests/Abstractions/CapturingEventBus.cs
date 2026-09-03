using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// Hand-rolled IEventBus test double -- no mocking library is referenced anywhere
/// in the solution (checked Directory.Packages.props), so this mirrors
/// ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions.CapturingEventBus
/// exactly rather than introducing one. Every integration event published via
/// PublishAsync is captured in CapturedEvents instead of being discarded.
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

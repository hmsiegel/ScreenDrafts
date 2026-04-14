using Microsoft.AspNetCore.SignalR;

using ScreenDrafts.Modules.RealTimeUpdates.Features;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

/// <summary>
/// Captures SignalR messages sent to <see cref="DraftHub"/> during scenario tests.
/// Registered as a singleton in <see cref="DraftsIntegrationTestWebAppFactory"/>,
/// replacing the real <c>IHubContext&lt;DraftHub&gt;</c>.
/// RealTimeUpdates consumers resolved in-process write their broadcast calls here.
/// Inspect <see cref="SentMessages"/> after
/// <see cref="Helpers.DraftScenarioBase.DispatchIntegrationEventsAsync"/>.
/// </summary>
public sealed class DraftHubCapture : IHubContext<DraftHub>
{
  private readonly RecordingHubClients _clients = new();

  public IHubClients Clients => _clients;

  /// <summary>
  /// Not used in scenario tests — throws if accessed.
  /// </summary>
  public IGroupManager Groups => throw new NotSupportedException(nameof(Groups));

  public IReadOnlyList<HubMessage> SentMessages => _clients.SentMessages;

  /// <summary>Discard all previously captured messages.</summary>
  public void Clear() => _clients.Clear();
}

/// <summary>A single SignalR broadcast captured by <see cref="DraftHubCapture"/>.</summary>
public sealed record HubMessage(string GroupName, string Method, object?[] Args);

internal sealed class RecordingHubClients : IHubClients
{
  private readonly List<HubMessage> _messages = [];

  public IReadOnlyList<HubMessage> SentMessages => _messages.AsReadOnly();

  public void Clear() => _messages.Clear();

  public IClientProxy Group(string groupName) => new RecordingGroupProxy(groupName, _messages);

  public IClientProxy All => StubHubProxy.Instance;
  public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => StubHubProxy.Instance;
  public IClientProxy Client(string connectionId) => StubHubProxy.Instance;
  public IClientProxy Clients(IReadOnlyList<string> connectionIds) => StubHubProxy.Instance;
  public IClientProxy Groups(IReadOnlyList<string> groupNames) => StubHubProxy.Instance;
  public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => StubHubProxy.Instance;
  public IClientProxy User(string userId) => StubHubProxy.Instance;
  public IClientProxy Users(IReadOnlyList<string> userIds) => StubHubProxy.Instance;
}

internal sealed class RecordingGroupProxy(string groupName, List<HubMessage> messages) : IClientProxy
{
  public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
  {
    messages.Add(new HubMessage(groupName, method, args));
    return Task.CompletedTask;
  }
}

internal sealed class StubHubProxy : IClientProxy
{
  public static readonly StubHubProxy Instance = new();

  public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    => Task.CompletedTask;
}

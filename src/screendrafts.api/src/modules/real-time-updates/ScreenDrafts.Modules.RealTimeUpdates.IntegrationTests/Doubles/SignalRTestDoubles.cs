namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.Doubles;

internal sealed record SentMessage(string GroupName, string Method, object?[] Args);

internal sealed class TestHubContext : IHubContext<DraftHub>
{
  private readonly RecordingHubClients _clients = new();

  public IHubClients Clients => _clients;

  public IGroupManager Groups => throw new NotSupportedException(nameof(Groups));

  public IReadOnlyList<SentMessage> SentMessages => _clients.SentMessages;
}

internal sealed class RecordingHubClients : IHubClients
{
  private readonly List<SentMessage> _messages = [];

  public IReadOnlyList<SentMessage> SentMessages => _messages;

  public IClientProxy Group(string groupName) => new RecordingProxy(groupName, _messages);

  public IClientProxy All => StubProxy.Instance;
  public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => StubProxy.Instance;
  public IClientProxy Client(string connectionId) => StubProxy.Instance;
  public IClientProxy Clients(IReadOnlyList<string> connectionIds) => StubProxy.Instance;
  public IClientProxy Groups(IReadOnlyList<string> groupNames) => StubProxy.Instance;
  public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => StubProxy.Instance;
  public IClientProxy User(string userId) => StubProxy.Instance;
  public IClientProxy Users(IReadOnlyList<string> userIds) => StubProxy.Instance;
}

internal sealed class RecordingProxy(string groupName, List<SentMessage> messages) : IClientProxy
{
  public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
  {
    messages.Add(new SentMessage(groupName, method, args));
    return Task.CompletedTask;
  }
}

internal sealed class StubProxy : IClientProxy
{
  public static readonly StubProxy Instance = new();

  public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    => Task.CompletedTask;
}

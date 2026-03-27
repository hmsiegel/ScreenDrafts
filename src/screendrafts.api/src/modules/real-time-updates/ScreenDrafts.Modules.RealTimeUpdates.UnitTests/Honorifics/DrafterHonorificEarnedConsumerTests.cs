namespace ScreenDrafts.Modules.RealTimeUpdates.UnitTests.Honorifics;

public sealed class DrafterHonorificEarnedConsumerTests
{
  // -------------------------------------------------------------------------
  // Handle — broadcasts to the correct SignalR group
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSendToCorrectGroup_BasedOnDraftPartPublicIdAsync()
  {
    // Arrange
    var hubContext = new TestHubContext();
    var consumer = new DrafterHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<DrafterHonorificEarnedIntegrationEventConsumer>.Instance);

    var draftPartPublicId = "dp-abc123";
    var integrationEvent = BuildEvent(draftPartPublicId: draftPartPublicId);

    // Act
    await consumer.Handle(integrationEvent);

    // Assert
    hubContext.LastGroupName.Should().Be(DraftHub.GroupName(draftPartPublicId));
  }

  [Fact]
  public async Task Handle_ShouldSendDrafterHonorificEarnedMethod_ToGroupAsync()
  {
    // Arrange
    var hubContext = new TestHubContext();
    var consumer = new DrafterHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<DrafterHonorificEarnedIntegrationEventConsumer>.Instance);

    var integrationEvent = BuildEvent();

    // Act
    await consumer.Handle(integrationEvent);

    // Assert
    hubContext.SentMessages.Should().ContainSingle()
      .Which.Method.Should().Be("DrafterHonorificEarned");
  }

  [Fact]
  public async Task Handle_ShouldPassCorrectArguments_ToClientAsync()
  {
    // Arrange
    var hubContext = new TestHubContext();
    var consumer = new DrafterHonorificEarnedIntegrationEventConsumer(
      hubContext,
      NullLogger<DrafterHonorificEarnedIntegrationEventConsumer>.Instance);

    var drafterId = Guid.NewGuid();
    var integrationEvent = BuildEvent(
      drafterId: drafterId,
      previousHonorific: 0,
      newHonorific: 1,
      appearanceCount: 5);

    // Act
    await consumer.Handle(integrationEvent);

    // Assert
    var (_, args) = hubContext.SentMessages.Single();
    args.Should().HaveCount(4);
    args[0].Should().Be(drafterId);
    args[1].Should().Be(0);
    args[2].Should().Be(1);
    args[3].Should().Be(5);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static DrafterHonorificEarnedIntegrationEvent BuildEvent(
    string draftPartPublicId = "dp-test",
    Guid? drafterId = null,
    int previousHonorific = 0,
    int newHonorific = 1,
    int appearanceCount = 5)
  {
    return new DrafterHonorificEarnedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      drafterIdValue: drafterId ?? Guid.NewGuid(),
      draftPartPublicId: draftPartPublicId,
      previousHonorificValue: previousHonorific,
      newHonorificValue: newHonorific,
      appearanceCount: appearanceCount);
  }
}

// ---------------------------------------------------------------------------
// Test doubles shared between consumer tests
// ---------------------------------------------------------------------------

internal sealed class TestHubContext : IHubContext<DraftHub>
{
  private readonly TestHubClients _clients = new();

  public IHubClients Clients => _clients;
  public IGroupManager Groups => null!; // not used by consumers

  public string? LastGroupName => _clients.LastGroupName;
  public IReadOnlyList<(string Method, object?[] Args)> SentMessages => _clients.SentMessages;
}

internal sealed class TestHubClients : IHubClients
{
  private readonly TestClientProxy _proxy = new();

  public string? LastGroupName { get; private set; }
  public IReadOnlyList<(string Method, object?[] Args)> SentMessages => _proxy.SentMessages;

  public IClientProxy All => _proxy;
  public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => _proxy;
  public IClientProxy Client(string connectionId) => _proxy;
  public IClientProxy Clients(IReadOnlyList<string> connectionIds) => _proxy;

  public IClientProxy Group(string groupName)
  {
    LastGroupName = groupName;
    return _proxy;
  }

  public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => _proxy;
  public IClientProxy Groups(IReadOnlyList<string> groupNames) => _proxy;
  public IClientProxy User(string userId) => _proxy;
  public IClientProxy Users(IReadOnlyList<string> userIds) => _proxy;
}

internal sealed class TestClientProxy : IClientProxy
{
  private readonly List<(string Method, object?[] Args)> _sentMessages = [];

  public IReadOnlyList<(string Method, object?[] Args)> SentMessages => _sentMessages;

  public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
  {
    _sentMessages.Add((method, args));
    return Task.CompletedTask;
  }
}

namespace ScreenDrafts.Modules.RealTimeUpdates.UnitTests.Hub;

public sealed class DraftHubTests
{
  // -------------------------------------------------------------------------
  // GroupName
  // -------------------------------------------------------------------------

  [Fact]
  public void GroupName_ShouldPrefixWithDraftPart()
  {
    DraftHub.GroupName("abc123").Should().Be("draft-part:abc123");
  }

  [Theory]
  [InlineData("id1", "draft-part:id1")]
  [InlineData("", "draft-part:")]
  [InlineData("some-guid-id", "draft-part:some-guid-id")]
  public void GroupName_ShouldFormatAllIds(string id, string expected)
  {
    DraftHub.GroupName(id).Should().Be(expected);
  }

  // -------------------------------------------------------------------------
  // JoinDraftPartAsync
  // -------------------------------------------------------------------------

  [Fact]
  public async Task JoinDraftPartAsync_ShouldAddConnectionToGroupAsync()
  {
    // Arrange
    const string connectionId = "conn-1";
    const string draftPartId = "dp-42";

    var groups = new TestGroupManager();
    using var hub = CreateHub(connectionId, groups);

    // Act
    await hub.JoinDraftPartAsync(draftPartId);

    // Assert
    groups.AddCalls.Should().ContainSingle()
      .Which.Should().Be((connectionId, draftPartId));
  }

  [Fact]
  public async Task JoinDraftPartAsync_ShouldNotRemoveFromAnyGroupAsync()
  {
    var groups = new TestGroupManager();
    using var hub = CreateHub("conn-1", groups);

    await hub.JoinDraftPartAsync("dp-1");

    groups.RemoveCalls.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // LeaveDraftPartAsync
  // -------------------------------------------------------------------------

  [Fact]
  public async Task LeaveDraftPartAsync_ShouldRemoveConnectionFromGroupAsync()
  {
    // Arrange
    const string connectionId = "conn-1";
    const string draftPartId = "dp-42";

    var groups = new TestGroupManager();
    using var hub = CreateHub(connectionId, groups);

    // Act
    await hub.LeaveDraftPartAsync(draftPartId);

    // Assert
    groups.RemoveCalls.Should().ContainSingle()
      .Which.Should().Be((connectionId, draftPartId));
  }

  [Fact]
  public async Task LeaveDraftPartAsync_ShouldNotAddToAnyGroupAsync()
  {
    var groups = new TestGroupManager();
    using var hub = CreateHub("conn-1", groups);

    await hub.LeaveDraftPartAsync("dp-1");

    groups.AddCalls.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static DraftHub CreateHub(string connectionId, IGroupManager groupManager)
  {
    var hub = new DraftHub();
    hub.Context = new TestHubCallerContext(connectionId);
    hub.Groups = groupManager;
    return hub;
  }
}

internal sealed class TestGroupManager : IGroupManager
{
  private readonly List<(string ConnectionId, string GroupName)> _addCalls = [];
  private readonly List<(string ConnectionId, string GroupName)> _removeCalls = [];

  public IReadOnlyList<(string ConnectionId, string GroupName)> AddCalls => _addCalls;
  public IReadOnlyList<(string ConnectionId, string GroupName)> RemoveCalls => _removeCalls;

  public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
  {
    _addCalls.Add((connectionId, groupName));
    return Task.CompletedTask;
  }

  public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
  {
    _removeCalls.Add((connectionId, groupName));
    return Task.CompletedTask;
  }
}

internal sealed class TestHubCallerContext(string connectionId) : HubCallerContext
{
  public override string ConnectionId { get; } = connectionId;
  public override string? UserIdentifier => null;
  public override ClaimsPrincipal? User => null;
  public override IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();
  public override IFeatureCollection Features { get; } = new FeatureCollection();
  public override CancellationToken ConnectionAborted => CancellationToken.None;
  public override void Abort() { }
}

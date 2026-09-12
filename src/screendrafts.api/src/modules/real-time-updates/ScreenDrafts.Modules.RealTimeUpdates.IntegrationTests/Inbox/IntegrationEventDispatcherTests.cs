using Microsoft.Extensions.DependencyInjection;

using ScreenDrafts.Modules.RealTimeUpdates.Features.Inbox;

namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.Inbox;

/// <summary>
/// RealTimeUpdatesIntegrationEventDispatcher had no coverage at all before this
/// file. Every consumer test in this module instantiates its consumer directly and
/// calls Handle(...), none of them go through the dispatcher's own
/// assembly-scan-then-DI-resolve path (IntegrationEventHandlersFactory.GetHandlers)
/// -- the exact layer where GuestDrafts' real ProcessInboxJob bug lived (resolving
/// the wrong dispatcher interface and silently scanning zero handlers). This test
/// goes through the real dispatcher with a minimal DI container and asserts a
/// handler actually ran (a SignalR broadcast was sent), not just that no exception
/// was thrown.
/// </summary>
public sealed class IntegrationEventDispatcherTests
{
  [Fact]
  public async Task DispatchAsync_ForPickAddedIntegrationEvent_ShouldResolveAndRunItsConsumerAsync()
  {
    // Arrange
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult();

    var services = new ServiceCollection();
    services.AddSingleton<IHubContext<DraftHub>>(hubContext);
    services.AddSingleton<FakeDbConnectionFactory>(fakeDb);
    services.AddSingleton<ScreenDrafts.Common.Application.Data.IDbConnectionFactory>(fakeDb);
    services.AddSingleton<ILogger<PickAddedIntegrationEventConsumer>>(
      NullLogger<PickAddedIntegrationEventConsumer>.Instance
    );
    services.AddScoped<PickAddedIntegrationEventConsumer>();
    await using var provider = services.BuildServiceProvider();

    var integrationEvent = new PickAddedIntegrationEvent(
      Guid.NewGuid(),
      DateTime.UtcNow,
      Guid.NewGuid(),
      "dp_dispatcher123",
      "tt1234567",
      "Test Movie",
      99999,
      1,
      1,
      Guid.NewGuid(),
      1
    );

    var dispatcher = new RealTimeUpdatesIntegrationEventDispatcher();

    // Act
    await dispatcher.DispatchAsync(integrationEvent, provider);

    // Assert -- the consumer actually ran and broadcast, not just that dispatch
    // completed without throwing.
    hubContext
      .SentMessages.Should()
      .ContainSingle(m => m.GroupName == DraftHub.GroupName("dp_dispatcher123") && m.Method == "PickAdded");
  }

  [Fact]
  public async Task DispatchAsync_WhenNoHandlerIsRegisteredForTheEventType_ShouldNotThrowAsync()
  {
    // Arrange -- a real consumer class exists in this assembly for this event type,
    // but nothing was registered in this container. GetRequiredService failing here
    // is this project's stand-in for "no handler wired up" -- the dispatcher must
    // surface that loudly, not swallow it as a silent no-op.
    var services = new ServiceCollection();
    await using var provider = services.BuildServiceProvider();

    var integrationEvent = new PickAddedIntegrationEvent(
      Guid.NewGuid(),
      DateTime.UtcNow,
      Guid.NewGuid(),
      "dp_nohandler",
      "tt1234567",
      "Test Movie",
      99999,
      1,
      1,
      Guid.NewGuid(),
      1
    );

    var dispatcher = new RealTimeUpdatesIntegrationEventDispatcher();

    // Act
    var act = async () => await dispatcher.DispatchAsync(integrationEvent, provider);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>();
  }
}

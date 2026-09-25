using ScreenDrafts.Common.Application.Clock;

namespace ScreenDrafts.Modules.Communications.IntegrationTests.Inbox;

/// <summary>
/// CommunicationsIntegrationEventDispatcher had no coverage at all before this file.
/// Every consumer test in this module (DraftCreatedConsumerTests, etc.) instantiates
/// its consumer directly and calls Handle(...) -- none of them go through the
/// dispatcher's own assembly-scan-then-DI-resolve path
/// (IntegrationEventHandlersFactory.GetHandlers), which is exactly the layer where
/// GuestDrafts' real ProcessInboxJob bug lived (resolving the wrong dispatcher
/// interface and silently scanning zero handlers). These tests go through the real
/// CommunicationsIntegrationEventDispatcher with a real (minimal) DI container, and
/// assert a handler actually ran via an observable side effect -- not just that no
/// exception was thrown.
/// </summary>
public sealed class IntegrationEventDispatcherTests
{
  [Fact]
  public async Task DispatchAsync_ForDraftCreatedIntegrationEvent_ShouldResolveAndRunItsConsumerAsync()
  {
    // Arrange
    var connectionFactory = new FakeDbConnectionFactory();
    connectionFactory.EnqueueQueryResult(
      ["EmailAddress", "FullName"],
      ["alice@example.com", "Alice"]
    );
    var emailService = new RecordingEmailService();

    var services = new ServiceCollection();
    services.AddSingleton<IDbConnectionFactory>(connectionFactory);
    services.AddSingleton<IEmailService>(emailService);
    services.AddSingleton<IDateTimeProvider>(new FakeDateTimeProvider(DateTime.UtcNow));
    services.AddScoped<DraftCreatedIntegrationEventConsumer>();
    await using var provider = services.BuildServiceProvider();

    var integrationEvent = new DraftCreatedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftId: Guid.NewGuid(),
      draftTitle: "Dispatcher Routing Test",
      isPatreon: false
    );

    var dispatcher = new CommunicationsIntegrationEventDispatcher();

    // Act
    await dispatcher.DispatchAsync(integrationEvent, provider);

    // Assert -- the consumer actually ran (queried recipients and sent an email),
    // not just that dispatch completed without throwing.
    connectionFactory.WasOpened.Should().BeTrue();
    emailService.SentEmails.Should().ContainSingle(e => e.ToAddress == "alice@example.com");
  }

  [Fact]
  public async Task DispatchAsync_ForUserRegisteredIntegrationEvent_ShouldResolveTheDifferentConsumerForThatEventTypeAsync()
  {
    // Arrange -- a second, distinct event/consumer pair, to prove the dispatcher's
    // routing is by event type rather than coincidentally resolving whatever is
    // registered.
    var connectionFactory = new FakeDbConnectionFactory();
    connectionFactory.EnqueueEmptyResult();

    var services = new ServiceCollection();
    services.AddSingleton<IDbConnectionFactory>(connectionFactory);
    services.AddScoped<UserRegisteredIntegrationEventConsumer>();
    await using var provider = services.BuildServiceProvider();

    var integrationEvent = new UserRegisteredIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      userId: Guid.NewGuid(),
      email: "new-user@example.com",
      firstName: "New",
      lastName: "User",
      middleName: null
    );

    var dispatcher = new CommunicationsIntegrationEventDispatcher();

    // Act
    await dispatcher.DispatchAsync(integrationEvent, provider);

    // Assert -- the upsert ran against the fake connection, proving this specific
    // event type routed to UserRegisteredIntegrationEventConsumer, not (say) a
    // silently-empty handler list.
    connectionFactory.WasOpened.Should().BeTrue();
    connectionFactory.ExecutedSql.Should().ContainSingle(sql => sql.Contains("communications.user_emails", StringComparison.Ordinal));
  }

  [Fact]
  public async Task DispatchAsync_WhenNoHandlerIsRegisteredForTheEventType_ShouldNotThrowAsync()
  {
    // Arrange -- an event type with a real consumer class in this assembly, but no
    // DI registration for it in this test's container (nothing was ever added). The
    // assembly scan still finds the concrete handler type; resolving it from an
    // empty container is this project's stand-in for "no handler wired up", and the
    // dispatcher must surface that as a normal exception, not a silent no-op that
    // hides a wiring problem.
    var services = new ServiceCollection();
    await using var provider = services.BuildServiceProvider();

    var integrationEvent = new DraftCreatedIntegrationEvent(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftId: Guid.NewGuid(),
      draftTitle: "No Handler Registered",
      isPatreon: false
    );

    var dispatcher = new CommunicationsIntegrationEventDispatcher();

    // Act
    var act = async () => await dispatcher.DispatchAsync(integrationEvent, provider);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>();
  }
}

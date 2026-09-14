namespace ScreenDrafts.Modules.Users.IntegrationTests.Users.EmailChange;

// UserEmailChangedDomainEventHandler / UserEmailChangeRequestedDomainEventHandler just
// translate a domain event into an integration event via IEventBus -- pure, no DB
// involved. Every integration test host in this repo replaces IEventBus with a
// NoOpEventBus (see IntegrationTestWebAppFactory.RemoveProblematicServices), so the
// translation can't be observed by going through Sender/HttpClient. These are
// internal Features-layer classes with no mocking-library seam in this codebase, so
// per repo convention this uses a small hand-rolled recording fake rather than Moq/NSubstitute.
public class EmailChangeDomainEventHandlersTests
{
  [Fact]
  public async Task UserEmailChangedHandler_ShouldPublishIntegrationEvent_WithMappedFieldsAsync()
  {
    // Arrange
    var eventBus = new RecordingEventBus();
    var now = new DateTime(2026, 9, 14, 12, 0, 0, DateTimeKind.Utc);
    var handler = new UserEmailChangedDomainEventHandler(eventBus, new FixedDateTimeProvider(now));
    var userId = Guid.NewGuid();
    var domainEvent = new UserEmailChangedDomainEvent(userId, "new@example.com");

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    var published = eventBus.PublishedEvents.Single().Should().BeOfType<UserEmailChangedIntegrationEvent>().Subject;
    published.UserId.Should().Be(userId);
    published.NewEmail.Should().Be("new@example.com");
    published.OccurredOnUtc.Should().Be(now);
  }

  [Fact]
  public async Task UserEmailChangeRequestedHandler_ShouldPublishIntegrationEvent_WithMappedFieldsAsync()
  {
    // Arrange
    var eventBus = new RecordingEventBus();
    var now = new DateTime(2026, 9, 14, 12, 0, 0, DateTimeKind.Utc);
    var handler = new UserEmailChangeRequestedDomainEventHandler(eventBus, new FixedDateTimeProvider(now));
    var userId = Guid.NewGuid();
    var domainEvent = new UserEmailChangeRequestedDomainEvent(
      userId,
      "new@example.com",
      "https://screendrafts.example/confirm?token=abc123"
    );

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    var published = eventBus
      .PublishedEvents.Single()
      .Should()
      .BeOfType<EmailChangeConfirmationRequestedIntegrationEvent>()
      .Subject;
    published.UserId.Should().Be(userId);
    published.NewEmail.Should().Be("new@example.com");
    published.ConfirmationLink.Should().Be("https://screendrafts.example/confirm?token=abc123");
    published.OccurredOnUtc.Should().Be(now);
  }

  private sealed class RecordingEventBus : IEventBus
  {
    private readonly List<IIntegrationEvent> _publishedEvents = [];

    public IReadOnlyList<IIntegrationEvent> PublishedEvents => _publishedEvents;

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
      where T : IIntegrationEvent
    {
      _publishedEvents.Add(integrationEvent);
      return Task.CompletedTask;
    }
  }

  private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
  {
    public DateTime UtcNow { get; } = utcNow;
    public DateTimeOffset UtcTimeZoneNow { get; } = utcNow;
  }
}

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.DomainEventHandlers;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
public sealed class GuestDraftStartedDomainEventHandlerTests(GuestDraftsIntegrationTestWebAppFactory factory)
{
  private readonly GuestDraftsIntegrationTestWebAppFactory _factory = factory;

  [Fact]
  public async Task Handle_ShouldPublishExactlyOneCorrectlyMappedGuestDraftStartedIntegrationEventAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftStartedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      participantCount: 4
    );
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftStartedDomainEventHandler(eventBus, new FakeDateTimeProvider(fixedUtcNow));

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    eventBus.CapturedEvents.Should().ContainSingle();
    var published = eventBus.CapturedEvents.Single().Should().BeOfType<GuestDraftStartedIntegrationEvent>().Subject;

    published.Id.Should().NotBe(Guid.Empty);
    published.OccurredOnUtc.Should().Be(fixedUtcNow);
    published.GuestDraftId.Should().Be(domainEvent.GuestDraftId);
    published.GuestDraftPublicId.Should().Be(domainEvent.GuestDraftPublicId);
    published.ParticipantCount.Should().Be(domainEvent.ParticipantCount);
  }

  [Fact]
  public async Task Handle_ShouldGenerateAFreshIdOnEachCallAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftStartedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      participantCount: 2
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftStartedDomainEventHandler(eventBus, new FakeDateTimeProvider(DateTime.UtcNow));

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    var ids = eventBus.CapturedEvents.Select(e => e.Id).ToList();
    ids.Should().HaveCount(2);
    ids[0].Should().NotBe(ids[1]);
  }

  [Fact]
  public void Handler_ShouldBeDiscoverableByGuestDraftsModulesDomainEventHandlerRegistration()
  {
    // Assert
    using var scope = _factory.Services.CreateScope();
    var handler = scope.ServiceProvider.GetService(typeof(GuestDraftStartedDomainEventHandler));

    handler.Should().NotBeNull();
    handler.Should().BeAssignableTo<IDomainEventHandler<GuestDraftStartedDomainEvent>>();
  }
}

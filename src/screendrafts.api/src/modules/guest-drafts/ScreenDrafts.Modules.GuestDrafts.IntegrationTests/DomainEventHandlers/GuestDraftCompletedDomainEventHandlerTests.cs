namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.DomainEventHandlers;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
public sealed class GuestDraftCompletedDomainEventHandlerTests(GuestDraftsIntegrationTestWebAppFactory factory)
{
  private readonly GuestDraftsIntegrationTestWebAppFactory _factory = factory;

  [Fact]
  public async Task Handle_ShouldPublishExactlyOneCorrectlyMappedGuestDraftCompletedIntegrationEventAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftCompletedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      totalPicks: 7,
      vetoCount: 2
    );
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftCompletedDomainEventHandler(eventBus, new FakeDateTimeProvider(fixedUtcNow));

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    eventBus.CapturedEvents.Should().ContainSingle();
    var published = eventBus.CapturedEvents.Single().Should().BeOfType<GuestDraftCompletedIntegrationEvent>().Subject;

    published.Id.Should().NotBe(Guid.Empty);
    published.OccurredOnUtc.Should().Be(fixedUtcNow);
    published.GuestDraftId.Should().Be(domainEvent.GuestDraftId);
    published.GuestDraftPublicId.Should().Be(domainEvent.GuestDraftPublicId);
    published.TotalPicks.Should().Be(domainEvent.TotalPicks);
    published.VetoCount.Should().Be(domainEvent.VetoCount);
    published.TotalPicks.Should().NotBe(published.VetoCount, "the fixture uses distinct values specifically to catch a copy-paste field swap");
  }

  [Fact]
  public async Task Handle_ShouldGenerateAFreshIdOnEachCallAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftCompletedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      totalPicks: 3,
      vetoCount: 1
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftCompletedDomainEventHandler(eventBus, new FakeDateTimeProvider(DateTime.UtcNow));

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
    var handler = scope.ServiceProvider.GetService(typeof(GuestDraftCompletedDomainEventHandler));

    handler.Should().NotBeNull();
    handler.Should().BeAssignableTo<IDomainEventHandler<GuestDraftCompletedDomainEvent>>();
  }
}

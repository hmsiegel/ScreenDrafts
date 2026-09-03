namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.DomainEventHandlers;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
public sealed class GuestDraftPickUndoneDomainEventHandlerTests(GuestDraftsIntegrationTestWebAppFactory factory)
{
  private readonly GuestDraftsIntegrationTestWebAppFactory _factory = factory;

  [Fact]
  public async Task Handle_ShouldPublishExactlyOneCorrectlyMappedGuestDraftPickUndoneIntegrationEventAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftPickUndoneDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      playOrder: 3,
      boardPosition: 5,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}"
    );
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftPickUndoneDomainEventHandler(eventBus, new FakeDateTimeProvider(fixedUtcNow));

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    eventBus.CapturedEvents.Should().ContainSingle();
    var published = eventBus.CapturedEvents.Single().Should().BeOfType<GuestDraftPickUndoneIntegrationEvent>().Subject;

    published.Id.Should().NotBe(Guid.Empty);
    published.OccurredOnUtc.Should().Be(fixedUtcNow);
    published.GuestDraftId.Should().Be(domainEvent.GuestDraftId);
    published.GuestDraftPublicId.Should().Be(domainEvent.GuestDraftPublicId);
    published.PlayOrder.Should().Be(domainEvent.PlayOrder);
    published.BoardPosition.Should().Be(domainEvent.BoardPosition);
    published.MoviePublicId.Should().Be(domainEvent.MoviePublicId);
  }

  [Fact]
  public async Task Handle_ShouldGenerateAFreshIdOnEachCallAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftPickUndoneDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      playOrder: 1,
      boardPosition: 1,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}"
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftPickUndoneDomainEventHandler(eventBus, new FakeDateTimeProvider(DateTime.UtcNow));

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
    var handler = scope.ServiceProvider.GetService(typeof(GuestDraftPickUndoneDomainEventHandler));

    handler.Should().NotBeNull();
    handler.Should().BeAssignableTo<IDomainEventHandler<GuestDraftPickUndoneDomainEvent>>();
  }
}

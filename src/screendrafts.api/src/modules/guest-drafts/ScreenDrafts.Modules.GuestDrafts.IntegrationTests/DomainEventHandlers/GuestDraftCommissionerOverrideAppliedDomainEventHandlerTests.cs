namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.DomainEventHandlers;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
public sealed class GuestDraftCommissionerOverrideAppliedDomainEventHandlerTests(GuestDraftsIntegrationTestWebAppFactory factory)
{
  private readonly GuestDraftsIntegrationTestWebAppFactory _factory = factory;

  [Fact]
  public async Task Handle_ShouldPublishExactlyOneCorrectlyMappedGuestDraftCommissionerOverrideAppliedIntegrationEventAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftCommissionerOverrideAppliedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 6,
      boardPosition: 4,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      playedByParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 1,
      overrideTokensRemaining: 2
    );
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftCommissionerOverrideAppliedDomainEventHandler(eventBus, new FakeDateTimeProvider(fixedUtcNow));

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    eventBus.CapturedEvents.Should().ContainSingle();
    var published = eventBus.CapturedEvents.Single().Should().BeOfType<GuestDraftCommissionerOverrideAppliedIntegrationEvent>().Subject;

    published.Id.Should().NotBe(Guid.Empty);
    published.OccurredOnUtc.Should().Be(fixedUtcNow);
    published.GuestDraftId.Should().Be(domainEvent.GuestDraftId);
    published.GuestDraftPublicId.Should().Be(domainEvent.GuestDraftPublicId);
    published.PickId.Should().Be(domainEvent.PickId);
    published.PlayOrder.Should().Be(domainEvent.PlayOrder);
    published.BoardPosition.Should().Be(domainEvent.BoardPosition);
    published.MoviePublicId.Should().Be(domainEvent.MoviePublicId);
    published.PlayedByParticipantId.Should().Be(domainEvent.PlayedByParticipantId);
    published.VetoTokensRemaining.Should().Be(domainEvent.VetoTokensRemaining);
    published.OverrideTokensRemaining.Should().Be(domainEvent.OverrideTokensRemaining);
  }

  [Fact]
  public async Task Handle_ShouldGenerateAFreshIdOnEachCallAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftCommissionerOverrideAppliedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 1,
      boardPosition: 1,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      playedByParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 0,
      overrideTokensRemaining: 0
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftCommissionerOverrideAppliedDomainEventHandler(eventBus, new FakeDateTimeProvider(DateTime.UtcNow));

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
    var handler = scope.ServiceProvider.GetService(typeof(GuestDraftCommissionerOverrideAppliedDomainEventHandler));

    handler.Should().NotBeNull();
    handler.Should().BeAssignableTo<IDomainEventHandler<GuestDraftCommissionerOverrideAppliedDomainEvent>>();
  }
}

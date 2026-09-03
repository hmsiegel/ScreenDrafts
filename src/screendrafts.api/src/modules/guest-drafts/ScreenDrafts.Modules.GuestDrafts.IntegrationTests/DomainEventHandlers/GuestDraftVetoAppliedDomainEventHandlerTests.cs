namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.DomainEventHandlers;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
public sealed class GuestDraftVetoAppliedDomainEventHandlerTests(GuestDraftsIntegrationTestWebAppFactory factory)
{
  private readonly GuestDraftsIntegrationTestWebAppFactory _factory = factory;

  [Fact]
  public async Task Handle_ShouldPublishExactlyOneCorrectlyMappedGuestDraftVetoAppliedIntegrationEventAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftVetoAppliedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 2,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      vetoedByParticipantId: Guid.NewGuid(),
      playedByParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 3,
      overrideTokensRemaining: 1
    );
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftVetoAppliedDomainEventHandler(eventBus, new FakeDateTimeProvider(fixedUtcNow));

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    eventBus.CapturedEvents.Should().ContainSingle();
    var published = eventBus.CapturedEvents.Single().Should().BeOfType<GuestDraftVetoAppliedIntegrationEvent>().Subject;

    published.Id.Should().NotBe(Guid.Empty);
    published.OccurredOnUtc.Should().Be(fixedUtcNow);
    published.GuestDraftId.Should().Be(domainEvent.GuestDraftId);
    published.GuestDraftPublicId.Should().Be(domainEvent.GuestDraftPublicId);
    published.PickId.Should().Be(domainEvent.PickId);
    published.PlayOrder.Should().Be(domainEvent.PlayOrder);
    published.MoviePublicId.Should().Be(domainEvent.MoviePublicId);
    published.VetoedByParticipantId.Should().Be(domainEvent.VetoedByParticipantId);
    published.PlayedByParticipantId.Should().Be(domainEvent.PlayedByParticipantId);
    published.VetoedByParticipantId.Should().NotBe(published.PlayedByParticipantId, "the fixture uses distinct values specifically to catch a copy-paste field swap");
    published.VetoTokensRemaining.Should().Be(domainEvent.VetoTokensRemaining);
    published.OverrideTokensRemaining.Should().Be(domainEvent.OverrideTokensRemaining);
  }

  [Fact]
  public async Task Handle_ShouldGenerateAFreshIdOnEachCallAsync()
  {
    // Arrange
    var domainEvent = new GuestDraftVetoAppliedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 1,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      vetoedByParticipantId: Guid.NewGuid(),
      playedByParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 0,
      overrideTokensRemaining: 0
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftVetoAppliedDomainEventHandler(eventBus, new FakeDateTimeProvider(DateTime.UtcNow));

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
    var handler = scope.ServiceProvider.GetService(typeof(GuestDraftVetoAppliedDomainEventHandler));

    handler.Should().NotBeNull();
    handler.Should().BeAssignableTo<IDomainEventHandler<GuestDraftVetoAppliedDomainEvent>>();
  }
}

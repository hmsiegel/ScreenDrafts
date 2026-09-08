using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.DomainEventHandlers;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
public sealed class GuestDraftPickPlayedDomainEventHandlerTests(
  GuestDraftsIntegrationTestWebAppFactory factory
)
{
  private readonly GuestDraftsIntegrationTestWebAppFactory _factory = factory;

  [Fact]
  public async Task Handle_ShouldPublishExactlyOneCorrectlyMappedGuestDraftPickSubmittedIntegrationEventAsync()
  {
    // Arrange
    var domainEvent = new PickPlayedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 3,
      boardPosition: 7,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      playedByParticipantId: Guid.NewGuid(),
      actedByPublicId: $"gdp_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      revealAuthorizedParticipantId: Guid.NewGuid()
    );
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftPickPlayedDomainEventHandler(
      eventBus,
      new FakeDateTimeProvider(fixedUtcNow)
    );

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    eventBus.CapturedEvents.Should().ContainSingle();
    var published = eventBus
      .CapturedEvents.Single()
      .Should()
      .BeOfType<GuestDraftPickSubmittedIntegrationEvent>()
      .Subject;

    published.Id.Should().NotBe(Guid.Empty);
    published.OccurredOnUtc.Should().Be(fixedUtcNow);
    published.GuestDraftId.Should().Be(domainEvent.GuestDraftId);
    published.GuestDraftPublicId.Should().Be(domainEvent.GuestDraftPublicId);
    published.PickId.Should().Be(domainEvent.PickId);
    published.PlayOrder.Should().Be(domainEvent.PlayOrder);
    published.BoardPosition.Should().Be(domainEvent.BoardPosition);
    published.MoviePublicId.Should().Be(domainEvent.MoviePublicId);
    published.PlayedByParticipantId.Should().Be(domainEvent.PlayedByParticipantId);
    published.ActedByPublicId.Should().Be(domainEvent.ActedByPublicId);
    published.RevealAuthorizedParticipantId.Should().Be(domainEvent.RevealAuthorizedParticipantId);
  }

  [Fact]
  public async Task Handle_ShouldGenerateAFreshIdOnEachCallAsync()
  {
    // Arrange
    var domainEvent = new PickPlayedDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 1,
      boardPosition: 1,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      playedByParticipantId: Guid.NewGuid(),
      actedByPublicId: null,
      revealAuthorizedParticipantId: null
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftPickPlayedDomainEventHandler(
      eventBus,
      new FakeDateTimeProvider(DateTime.UtcNow)
    );

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
    // Assert -- resolving the concrete handler type from the real, module-composed
    // DI container catches a handler that's silently never registered, which a
    // hand-constructed `new Handler(...)` call in the tests above cannot.
    using var scope = _factory.Services.CreateScope();
    var handler = scope.ServiceProvider.GetService(typeof(GuestDraftPickPlayedDomainEventHandler));

    handler.Should().NotBeNull();
    handler.Should().BeAssignableTo<IDomainEventHandler<PickPlayedDomainEvent>>();
  }
}

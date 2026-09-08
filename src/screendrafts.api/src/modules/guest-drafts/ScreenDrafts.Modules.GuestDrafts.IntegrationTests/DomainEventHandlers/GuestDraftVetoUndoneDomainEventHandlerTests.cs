using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.DomainEvents;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.DomainEventHandlers;

[Collection(nameof(GuestDraftsIntegrationTestCollection))]
public sealed class GuestDraftVetoUndoneDomainEventHandlerTests(
  GuestDraftsIntegrationTestWebAppFactory factory
)
{
  private readonly GuestDraftsIntegrationTestWebAppFactory _factory = factory;

  [Fact]
  public async Task Handle_ShouldPublishExactlyOneCorrectlyMappedGuestDraftVetoUndoneIntegrationEventAsync()
  {
    // Arrange -- "fully populated" here includes the nullable refund/token fields,
    // since GuestDraftVetoUndoneDomainEvent's normal (non-edge-case) path always has
    // a resolvable issuer -- only FindParticipant returning null makes them null.
    var domainEvent = new VetoUndoneDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 2,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      refundedToParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 1,
      overrideTokensRemaining: 0
    );
    var fixedUtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftVetoUndoneDomainEventHandler(
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
      .BeOfType<GuestDraftVetoUndoneIntegrationEvent>()
      .Subject;

    published.Id.Should().NotBe(Guid.Empty);
    published.OccurredOnUtc.Should().Be(fixedUtcNow);
    published.GuestDraftId.Should().Be(domainEvent.GuestDraftId);
    published.GuestDraftPublicId.Should().Be(domainEvent.GuestDraftPublicId);
    published.PickId.Should().Be(domainEvent.PickId);
    published.PlayOrder.Should().Be(domainEvent.PlayOrder);
    published.MoviePublicId.Should().Be(domainEvent.MoviePublicId);
    published.RefundedToParticipantId.Should().Be(domainEvent.RefundedToParticipantId);
    published.VetoTokensRemaining.Should().Be(domainEvent.VetoTokensRemaining);
    published.OverrideTokensRemaining.Should().Be(domainEvent.OverrideTokensRemaining);
  }

  [Fact]
  public async Task Handle_ShouldMapNullRefundFieldsThrough_WhenTheIssuerCouldNotBeResolvedAsync()
  {
    // Arrange -- the domain comment on GuestDraftVetoUndoneDomainEvent calls this
    // out explicitly: a null pair means "no token update available," not zero, so
    // the handler must pass the nulls through rather than coercing them.
    var domainEvent = new VetoUndoneDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 2,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      refundedToParticipantId: null,
      vetoTokensRemaining: null,
      overrideTokensRemaining: null
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftVetoUndoneDomainEventHandler(
      eventBus,
      new FakeDateTimeProvider(DateTime.UtcNow)
    );

    // Act
    await handler.Handle(domainEvent, CancellationToken.None);

    // Assert
    var published = eventBus
      .CapturedEvents.Single()
      .Should()
      .BeOfType<GuestDraftVetoUndoneIntegrationEvent>()
      .Subject;
    published.RefundedToParticipantId.Should().BeNull();
    published.VetoTokensRemaining.Should().BeNull();
    published.OverrideTokensRemaining.Should().BeNull();
  }

  [Fact]
  public async Task Handle_ShouldGenerateAFreshIdOnEachCallAsync()
  {
    // Arrange
    var domainEvent = new VetoUndoneDomainEvent(
      guestDraftId: Guid.NewGuid(),
      guestDraftPublicId: $"gd_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      pickId: Guid.NewGuid(),
      playOrder: 1,
      moviePublicId: $"m_{TestFakerProvider.Faker.Random.AlphaNumeric(15)}",
      refundedToParticipantId: Guid.NewGuid(),
      vetoTokensRemaining: 1,
      overrideTokensRemaining: 0
    );
    var eventBus = new CapturingEventBus();
    var handler = new GuestDraftVetoUndoneDomainEventHandler(
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
    // Assert
    using var scope = _factory.Services.CreateScope();
    var handler = scope.ServiceProvider.GetService(typeof(GuestDraftVetoUndoneDomainEventHandler));

    handler.Should().NotBeNull();
    handler.Should().BeAssignableTo<IDomainEventHandler<VetoUndoneDomainEvent>>();
  }
}

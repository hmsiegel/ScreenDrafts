using Microsoft.Extensions.Options;

namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.DraftParts;

/// <summary>
/// DraftPartCompletedIntegrationEventConsumer had no coverage at all before this
/// file, despite being the most complex consumer in the module: it gates two
/// separate honorific-threshold queries behind an isCanonical check (Patreon /
/// non-canonical policies must skip both entirely, not just omit them from the
/// payload), then assembles predictions and standings. These tests cover the
/// isCanonical gate and the movie-honorific threshold; the predictions/standings
/// assembly (several more joined tables) is flagged as a further gap rather than
/// guessed at.
/// </summary>
public sealed class DraftPartCompletedConsumerTests
{
  private static DraftPartCompletedIntegrationEventConsumer BuildConsumer(
    TestHubContext hubContext,
    FakeDbConnectionFactory fakeDb
  ) =>
    new(
      hubContext,
      NullLogger<DraftPartCompletedIntegrationEventConsumer>.Instance,
      fakeDb,
      Options.Create(new RealTimeUpdatesDraftsOptions())
    );

  private static DraftPartCompletedIntegrationEvent BuildEvent(
    string draftPartPublicId = "dp_completed123",
    int canonicalPolicyValue = 0,
    IReadOnlyList<CompletedPickRecord>? picks = null,
    IReadOnlyList<string>? participantPublicIds = null
  ) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      draftId: Guid.NewGuid(),
      draftPublicId: "d_completed123",
      draftPartPublicId: draftPartPublicId,
      partIndex: 1,
      totalParts: 1,
      totalPicks: 7,
      title: "Test Draft",
      draftType: "Standard",
      isPatreon: false,
      episodeNumber: null,
      vetoCount: 0,
      picks: picks ?? [],
      participantPublicIds: participantPublicIds ?? [],
      subDraftBreakdowns: null,
      canonicalPolicyValue: canonicalPolicyValue
    );

  [Fact]
  public async Task Handle_WithNoPicksOrParticipants_ShouldBroadcastPartCompletedToTheCorrectGroupAsync()
  {
    // Arrange
    var draftPartPublicId = "dp_completed123";
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult(); // predictionSql always runs
    var consumer = BuildConsumer(hubContext, fakeDb);

    // Act
    await consumer.Handle(BuildEvent(draftPartPublicId), CancellationToken.None);

    // Assert
    hubContext
      .SentMessages.Should()
      .ContainSingle()
      .Which.GroupName.Should()
      .Be(DraftHub.GroupName(draftPartPublicId));
    hubContext.SentMessages.Single().Method.Should().Be("PartCompleted");
  }

  [Fact]
  public async Task Handle_WhenCanonicalAndMoviePriorAppearanceExists_ShouldIncludeItInMovieHonorificsAsync()
  {
    // Arrange -- prior count of 1 pushes the movie to 2 appearances, which is the
    // MarqueeOfFame threshold; the handler must include it.
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueQueryResult(["MoviePublicId", "PriorCount"], ["m_shining", 1]);
    fakeDb.EnqueueEmptyResult(); // predictionSql
    var consumer = BuildConsumer(hubContext, fakeDb);
    var pick = new CompletedPickRecord(1, "m_shining", "The Shining");

    // Act
    await consumer.Handle(
      BuildEvent(canonicalPolicyValue: 0, picks: [pick]),
      CancellationToken.None
    );

    // Assert
    dynamic payload = hubContext.SentMessages.Single().Args[0]!;
    var movieHonorifics = (IEnumerable<PickHonorificRecord>)payload.MovieHonorifics;
    movieHonorifics.Should().ContainSingle(h => h.MediaPublicId == "m_shining" && h.NewCount == 2);
  }

  [Fact]
  public async Task Handle_WhenNotCanonical_ShouldSkipBothHonorificQueriesEntirelyAsync()
  {
    // Arrange -- regression: CanonicalPolicyValue 1/2 (Patreon-style policies) must
    // gate the movie AND drafter honorific queries off entirely, not just omit them
    // from the payload after querying anyway.
    var hubContext = new TestHubContext();
    var fakeDb = new FakeDbConnectionFactory();
    fakeDb.EnqueueEmptyResult(); // only predictionSql should run
    var consumer = BuildConsumer(hubContext, fakeDb);
    var pick = new CompletedPickRecord(1, "m_shining", "The Shining");

    // Act
    await consumer.Handle(
      BuildEvent(canonicalPolicyValue: 1, picks: [pick], participantPublicIds: ["p_someone"]),
      CancellationToken.None
    );

    // Assert -- no query against either honorific table was ever issued
    fakeDb
      .ExecutedSql.Should()
      .NotContain(sql => sql.Contains("movie_canonical_picks", StringComparison.Ordinal));
    fakeDb
      .ExecutedSql.Should()
      .NotContain(sql => sql.Contains("drafter_canonical_appearances", StringComparison.Ordinal));
    dynamic payload = hubContext.SentMessages.Single().Args[0]!;
    ((IEnumerable<PickHonorificRecord>)payload.MovieHonorifics).Should().BeEmpty();
    ((IEnumerable<DrafterHonorificRecord>)payload.DrafterHonorifics).Should().BeEmpty();
  }
}

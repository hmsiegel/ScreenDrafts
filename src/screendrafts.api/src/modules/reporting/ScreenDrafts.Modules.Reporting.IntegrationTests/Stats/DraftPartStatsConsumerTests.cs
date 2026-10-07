using static ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions.StatsEvents;

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Stats;

public sealed class DraftPartStatsConsumerTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Guid _draftId = new(9, 9, 9, new byte[8]);

  private DraftPartStatsRecordedIntegrationEventConsumer Consumer() =>
    new(Sender, NullLogger<DraftPartStatsRecordedIntegrationEventConsumer>.Instance);

  private Task HandleAsync(DraftPartStatsRecordedIntegrationEvent stats) =>
    Consumer().Handle(stats, TestContext.Current.CancellationToken);

  private Task<List<DrafterCanonicalAppearance>> AppearancesAsync(Guid drafterId) =>
    DbContext
      .DrafterCanonicalAppearances.AsNoTracking()
      .Where(a => a.DrafterIdValue == drafterId)
      .ToListAsync(TestContext.Current.CancellationToken);

  private Task<DrafterHonorificEntity?> HonorificAsync(Guid drafterId) =>
    DbContext
      .DrafterHonorifics.AsNoTracking()
      .FirstOrDefaultAsync(h => h.DrafterIdValue == drafterId, TestContext.Current.CancellationToken);

  [Fact]
  public async Task Handle_ShouldCreateAppearanceWithTheEventsDraftId_ForEachCreditedDrafterAsync()
  {
    var stats = Event("dp_c1", _draftId, [SoloPick(1), SoloPick(2)]);

    await HandleAsync(stats);

    var first = (await AppearancesAsync(StatsSeeder.DrafterId(1))).Should().ContainSingle().Subject;
    first.DraftId.Should().Be(_draftId);
    first.DraftPartPublicId.Should().Be("dp_c1");
    (await AppearancesAsync(StatsSeeder.DrafterId(2))).Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldGiveEveryTeamMemberAnAppearanceAsync()
  {
    var team = Pick(o =>
    {
      o.PlayedByKind = 1;
      o.PlayedById = StatsSeeder.TeamId(1);
      o.Credits = [Credit(3), Credit(4)];
    });

    await HandleAsync(Event("dp_c2", _draftId, [team]));

    (await AppearancesAsync(StatsSeeder.DrafterId(3))).Should().ContainSingle();
    (await AppearancesAsync(StatsSeeder.DrafterId(4))).Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldSendOneHonorificCommandPerDistinctDrafterAsync()
  {
    // Drafter 1 holds two picks in the part but is one drafter.
    await HandleAsync(Event("dp_c3", _draftId, [SoloPick(1), SoloPick(1, o => o.PlayOrder = 2)]));

    (await AppearancesAsync(StatsSeeder.DrafterId(1))).Should().ContainSingle();
    (await HonorificAsync(StatsSeeder.DrafterId(1)))!.AppearanceCount.Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldRecordFactsButCreateNoAppearances_WhenPolicyIsNeverCanonicalAsync()
  {
    var stats = Event("dp_c4", _draftId, [SoloPick(1)], o => o.CanonicalPolicy = 1);

    await HandleAsync(stats);

    (await AppearancesAsync(StatsSeeder.DrafterId(1))).Should().BeEmpty();
    (await HonorificAsync(StatsSeeder.DrafterId(1))).Should().BeNull();
    (await DbContext.PickFacts.CountAsync(p => p.DraftPartPublicId == "dp_c4", TestContext.Current.CancellationToken)).Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldNotDoubleCount_WhenTheEventIsRedeliveredAsync()
  {
    var stats = Event("dp_c5", _draftId, [SoloPick(1)]);

    await HandleAsync(stats);
    await HandleAsync(stats);

    (await AppearancesAsync(StatsSeeder.DrafterId(1))).Should().ContainSingle();
    (await HonorificAsync(StatsSeeder.DrafterId(1)))!.AppearanceCount.Should().Be(1);
    (await DbContext.PickFacts.CountAsync(p => p.DraftPartPublicId == "dp_c5", TestContext.Current.CancellationToken)).Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldCountADraftOnce_WhenTheDrafterIsInTwoPartsOfItAsync()
  {
    await HandleAsync(Event("dp_c6a", _draftId, [SoloPick(1)], o => o.PartIndex = 1));
    await HandleAsync(Event("dp_c6b", _draftId, [SoloPick(1)], o => o.PartIndex = 2));

    (await AppearancesAsync(StatsSeeder.DrafterId(1))).Should().HaveCount(2);
    (await HonorificAsync(StatsSeeder.DrafterId(1)))!.AppearanceCount.Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldCountEachDraftOnce_WhenTheDrafterIsInTwoDraftsAsync()
  {
    await HandleAsync(Event("dp_c7a", _draftId, [SoloPick(1)]));
    await HandleAsync(Event("dp_c7b", Guid.NewGuid(), [SoloPick(1)]));

    (await HonorificAsync(StatsSeeder.DrafterId(1)))!.AppearanceCount.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldNotCountTowardTheHonorific_WhenPolicyIsOnMainFeedAndThereIsNoReleaseAsync()
  {
    var stats = Event("dp_c8", _draftId, [SoloPick(1)], o => { o.CanonicalPolicy = 2; o.HasMainFeedRelease = false; });

    await HandleAsync(stats);

    (await AppearancesAsync(StatsSeeder.DrafterId(1))).Should().ContainSingle();
    (await HonorificAsync(StatsSeeder.DrafterId(1)))!.AppearanceCount.Should().Be(0);
  }

  [Fact]
  public async Task Handle_ShouldNotGiveCommunityPicksAnAppearanceAsync()
  {
    var community = Pick(o =>
    {
      o.PlayedByKind = 2;
      o.PlayedById = Guid.Empty;
    });

    await HandleAsync(Event("dp_c9", _draftId, [community]));

    (await DbContext.DrafterCanonicalAppearances.CountAsync(TestContext.Current.CancellationToken)).Should().Be(0);
  }
}

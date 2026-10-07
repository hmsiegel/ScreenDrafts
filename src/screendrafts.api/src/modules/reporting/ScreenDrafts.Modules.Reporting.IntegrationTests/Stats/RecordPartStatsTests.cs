using static ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions.StatsEvents;

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Stats;

public sealed class RecordPartStatsTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Guid _draftId = new(7, 7, 7, new byte[8]);
  private const string PartId = "dp_recordpart1";

  private Task<Result> SendAsync(DraftPartStatsRecordedIntegrationEvent stats) =>
    Sender.Send(new RecordPartStatsCommand { Stats = stats }, TestContext.Current.CancellationToken);

  private Task<List<PickFact>> PicksAsync(string part = PartId) =>
    DbContext
      .PickFacts.AsNoTracking()
      .Where(p => p.DraftPartPublicId == part)
      .ToListAsync(TestContext.Current.CancellationToken);

  private Task<List<VetoFact>> VetoesAsync(string part = PartId) =>
    DbContext
      .VetoFacts.AsNoTracking()
      .Where(v => v.DraftPartPublicId == part)
      .ToListAsync(TestContext.Current.CancellationToken);

  private Task<List<PickCreditFact>> CreditsAsync(string part = PartId) =>
    DbContext
      .PickCreditFacts.AsNoTracking()
      .Where(c => c.DraftPartPublicId == part)
      .ToListAsync(TestContext.Current.CancellationToken);

  // -------------------------------------------------------------------------
  // Inserts
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldInsertPickVetoAndCreditRows_WhenAnEventIsRecordedAsync()
  {
    var pickId = Guid.NewGuid();
    var vetoer = StatsSeeder.DrafterId(2);

    var stats = Event(
      PartId,
      _draftId,
      [
        SoloPick(1, o =>
        {
          o.PickId = pickId;
          o.Position = 4;
          o.PlayOrder = 9;
          o.SubDraftIndex = 2;
          o.Vetoes = [Veto(1, 0, vetoer)];
        }),
        SoloPick(2, o => o.MediaPublicId = "m_alien"),
      ],
      o =>
      {
        o.CanonicalPolicy = 2;
        o.SeriesName = "Bonus Series";
        o.DraftType = "Mega";
        o.PartIndex = 3;
      });

    var result = await SendAsync(stats);

    result.IsSuccess.Should().BeTrue();

    var picks = await PicksAsync();
    picks.Should().HaveCount(2);

    var pick = picks.Single(p => p.Id == pickId);
    pick.DraftId.Should().Be(_draftId);
    pick.Position.Should().Be(4);
    pick.PlayOrder.Should().Be(9);
    pick.SubDraftIndex.Should().Be(2);
    pick.CanonicalPolicy.Should().Be(2);
    pick.SeriesName.Should().Be("Bonus Series");
    pick.DraftType.Should().Be("Mega");
    pick.PartIndex.Should().Be(3);
    pick.VetoCount.Should().Be(1);
    pick.WasVetoed.Should().BeTrue();

    (await VetoesAsync()).Should().ContainSingle().Which.PickId.Should().Be(pickId);
    (await CreditsAsync()).Should().HaveCount(2);
  }

  // -------------------------------------------------------------------------
  // Replace, not append
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldLeaveExactlyOneSetOfRows_WhenRunTwiceForTheSamePartAsync()
  {
    var vetoer = StatsSeeder.DrafterId(2);
    var stats = Event(PartId, _draftId, [SoloPick(1, o => o.Vetoes = [Veto(1, 0, vetoer)]), SoloPick(2)]);

    await SendAsync(stats);
    await SendAsync(stats);

    (await PicksAsync()).Should().HaveCount(2);
    (await VetoesAsync()).Should().HaveCount(1);
    (await CreditsAsync()).Should().HaveCount(2);
  }

  [Fact]
  public async Task Handle_ShouldReplaceTheOldRows_WhenTheSecondEventHasDifferentContentAsync()
  {
    var first = Event(PartId, _draftId, [SoloPick(1), SoloPick(2), SoloPick(3)]);
    var onlyPick = SoloPick(4, o => o.MediaTitle = "Only");
    var second = Event(PartId, _draftId, [onlyPick]);

    await SendAsync(first);
    await SendAsync(second);

    var picks = await PicksAsync();
    picks.Should().ContainSingle().Which.Id.Should().Be(onlyPick.PickId);
    (await CreditsAsync()).Should().ContainSingle().Which.DrafterIdValue.Should().Be(StatsSeeder.DrafterId(4));
  }

  [Fact]
  public async Task Handle_ShouldNotTouchOtherParts_WhenReplacingOnePartAsync()
  {
    await SendAsync(Event("dp_other", _draftId, [SoloPick(1), SoloPick(2)]));

    await SendAsync(Event(PartId, _draftId, [SoloPick(3)]));
    await SendAsync(Event(PartId, _draftId, [SoloPick(4)]));

    (await PicksAsync("dp_other")).Should().HaveCount(2);
    (await CreditsAsync("dp_other")).Should().HaveCount(2);
  }

  // -------------------------------------------------------------------------
  // Veto flags
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSetWasVetoOverridden_FromTheHighestSequenceVetoAsync()
  {
    var issuer = StatsSeeder.DrafterId(2);
    var overrider = StatsSeeder.DrafterId(3);

    // Veto 1 was overridden, then veto 2 (the override of the override) stood. Listed out of order on purpose.
    var vetoedAgain = SoloPick(1, o => o.Vetoes =
    [
      Veto(2, 0, issuer),
      Veto(1, 0, issuer, overridden: true, overriddenBy: overrider),
    ]);

    // Single veto that was overridden.
    var saved = SoloPick(1, o => o.Vetoes = [Veto(1, 0, issuer, overridden: true, overriddenBy: overrider)]);

    // Veto 1 stood, veto 2 overridden.
    var lastOverridden = SoloPick(1, o => o.Vetoes =
    [
      Veto(1, 0, issuer),
      Veto(2, 0, issuer, overridden: true, overriddenBy: overrider),
    ]);

    await SendAsync(Event(PartId, _draftId, [vetoedAgain, saved, lastOverridden]));

    var picks = await PicksAsync();
    var byId = picks.ToDictionary(p => p.Id);

    byId[vetoedAgain.PickId].WasVetoOverridden.Should().BeFalse();
    byId[vetoedAgain.PickId].VetoCount.Should().Be(2);
    byId[saved.PickId].WasVetoOverridden.Should().BeTrue();
    byId[lastOverridden.PickId].WasVetoOverridden.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldLeaveVetoFlagsFalse_WhenAPickHasNoVetoesAsync()
  {
    var plain = SoloPick(1);

    await SendAsync(Event(PartId, _draftId, [plain]));

    var pick = (await PicksAsync()).Single();
    pick.VetoCount.Should().Be(0);
    pick.WasVetoed.Should().BeFalse();
    pick.WasVetoOverridden.Should().BeFalse();
  }

  [Fact]
  public async Task Handle_ShouldFlagCommissionerOverriddenPicksAsync()
  {
    await SendAsync(Event(PartId, _draftId, [SoloPick(1, o => o.Removed = true), SoloPick(2)]));

    var picks = await PicksAsync();
    picks.Count(p => p.WasCommissionerOverridden).Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldMarkSelfVeto_WhenIssuerKindAndIdMatchThePlayerAsync()
  {
    var self = SoloPick(1, o => o.Vetoes = [Veto(1, 0, StatsSeeder.DrafterId(1))]);
    var other = SoloPick(1, o => o.Vetoes = [Veto(1, 0, StatsSeeder.DrafterId(2))]);
    // Same id as the player but a different kind (team vs drafter) is not a self-veto.
    var sameIdOtherKind = SoloPick(1, o => o.Vetoes = [Veto(1, 1, StatsSeeder.DrafterId(1))]);
    var community = SoloPick(1, o => o.Vetoes = [Veto(1, 2, Guid.Empty)]);

    await SendAsync(Event(PartId, _draftId, [self, other, sameIdOtherKind, community]));

    var vetoes = await VetoesAsync();
    var byPick = vetoes.ToDictionary(v => v.PickId);

    byPick[self.PickId].IsSelfVeto.Should().BeTrue();
    byPick[other.PickId].IsSelfVeto.Should().BeFalse();
    byPick[sameIdOtherKind.PickId].IsSelfVeto.Should().BeFalse();
    byPick[community.PickId].IsSelfVeto.Should().BeFalse();
  }

  [Fact]
  public async Task Handle_ShouldStoreVetoIssuerAndOverriderDetailsAsync()
  {
    var issuer = StatsSeeder.DrafterId(2);
    var overrider = StatsSeeder.DrafterId(3);
    var pick = SoloPick(1, o => o.Vetoes = [Veto(1, 0, issuer, overridden: true, overriddenBy: overrider)]);

    await SendAsync(Event(PartId, _draftId, [pick]));

    var veto = (await VetoesAsync()).Single();
    veto.IssuedByKind.Should().Be(0);
    veto.IssuedByIdValue.Should().Be(issuer);
    veto.IsOverridden.Should().BeTrue();
    veto.OverriddenByKind.Should().Be(0);
    veto.OverriddenByIdValue.Should().Be(overrider);
    veto.Sequence.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Credits
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldInsertNoCreditRows_WhenAPickHasNoCreditsAsync()
  {
    var community = Pick(o =>
    {
      o.PlayedByKind = 2;
      o.PlayedById = Guid.Empty;
      o.PlayedByName = "Patreon Members";
    });

    await SendAsync(Event(PartId, _draftId, [community]));

    (await PicksAsync()).Should().HaveCount(1);
    (await CreditsAsync()).Should().BeEmpty();
  }

  [Fact]
  public async Task Handle_ShouldCreditEveryTeamMember_AndStorePersonPublicIdAsync()
  {
    var team = Pick(o =>
    {
      o.PlayedByKind = 1;
      o.PlayedById = StatsSeeder.TeamId(1);
      o.PlayedByName = "Team 1";
      o.Credits = [Credit(1), Credit(2)];
    });

    await SendAsync(Event(PartId, _draftId, [team]));

    var credits = await CreditsAsync();
    credits.Select(c => c.DrafterIdValue).Should().BeEquivalentTo([StatsSeeder.DrafterId(1), StatsSeeder.DrafterId(2)]);
    credits.Select(c => c.DrafterPersonPublicId).Should().BeEquivalentTo(StatsSeeder.PersonPublicId(1), StatsSeeder.PersonPublicId(2));
  }

  [Fact]
  public async Task Handle_ShouldInsertOneCredit_WhenTheSameDrafterIsCreditedTwiceOnAPickAsync()
  {
    var pick = Pick(o => o.Credits = [Credit(1), Credit(1)]);

    await SendAsync(Event(PartId, _draftId, [pick]));

    (await CreditsAsync()).Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldStoreAnEmptyPersonPublicId_WhenCreditHasNoneAsync()
  {
    var credit = new StatsCreditRecord(StatsSeeder.DrafterId(1), "dr_1", null!, "Drafter 01");
    var pick = Pick(o => o.Credits = [credit]);

    await SendAsync(Event(PartId, _draftId, [pick]));

    (await CreditsAsync()).Single().DrafterPersonPublicId.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Cache
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldRemoveBothRecordBookCacheKeys_AfterCommitAsync()
  {
    var cache = GetService<IDistributedCache>();
    await cache.SetStringAsync(ReportingCacheKeys.RecordBookCanonicalCacheKey, "stale", TestContext.Current.CancellationToken);
    await cache.SetStringAsync(ReportingCacheKeys.RecordBookAllCacheKey, "stale", TestContext.Current.CancellationToken);

    await SendAsync(Event(PartId, _draftId, [SoloPick(1)]));

    (await cache.GetStringAsync(ReportingCacheKeys.RecordBookCanonicalCacheKey, TestContext.Current.CancellationToken)).Should().BeNull();
    (await cache.GetStringAsync(ReportingCacheKeys.RecordBookAllCacheKey, TestContext.Current.CancellationToken)).Should().BeNull();
    (await PicksAsync()).Should().HaveCount(1);
  }

  [Fact]
  public async Task Handle_ShouldSucceed_WhenTheEventHasNoPicksAsync()
  {
    await SendAsync(Event(PartId, _draftId, [SoloPick(1)]));

    var result = await SendAsync(Event(PartId, _draftId, []));

    result.IsSuccess.Should().BeTrue();
    (await PicksAsync()).Should().BeEmpty();
  }
}

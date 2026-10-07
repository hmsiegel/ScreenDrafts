using static ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions.StatsSeeder;

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Stats;

public sealed class GetRecordBookTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly DateOnly _jan1 = new(2026, 1, 1);

  private async Task<GetRecordBookResponse> RecordBookAsync(bool includeAll = false)
  {
    var result = await Sender.Send(
      new GetRecordBookQuery { IncludeAll = includeAll },
      TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    return result.Value;
  }

  private static int Total(GetRecordBookResponse response, string code) =>
    response.Totals.Single(t => t.Code == code).Value;

  private static RecordItem? Record(GetRecordBookResponse response, string code) =>
    response.Sections.SelectMany(s => s.Groups).SelectMany(g => g.Records).FirstOrDefault(r => r.Code == code);

  private static string[] Holders(RecordItem? item) => [.. item?.Holders.Select(h => h.Name) ?? []];

  private async Task ClearRecordBookCacheAsync()
  {
    var cache = GetService<ICacheService>();
    await cache.RemoveAsync(ReportingCacheKeys.RecordBookCanonicalCacheKey, TestContext.Current.CancellationToken);
    await cache.RemoveAsync(ReportingCacheKeys.RecordBookAllCacheKey, TestContext.Current.CancellationToken);
  }

  /// <summary>One clean pick in a single-part draft of the given policy.</summary>
  private static void SimpleDraft(StatsSeeder seed, string title, int policy, DateOnly? release = null)
  {
    var part = seed.Draft(title, policy: policy).Part(episode: null, mainFeed: release);
    part.Pick("Filler " + title, position: 1, playedBy: 1);
  }

  // -------------------------------------------------------------------------
  // Scope
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldIncludeOnlyCanonicalDrafts_WhenIncludeAllIsFalseAsync()
  {
    var seed = new StatsSeeder(DbContext);
    SimpleDraft(seed, "Policy zero", policy: 0);
    SimpleDraft(seed, "Policy two released", policy: 2, release: _jan1);
    SimpleDraft(seed, "Policy two unreleased", policy: 2);
    SimpleDraft(seed, "Policy one", policy: 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    response.IncludesNonCanonical.Should().BeFalse();
    Total(response, "drafts").Should().Be(2);
    Total(response, "picks-made").Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldIncludeEveryDraft_WhenIncludeAllIsTrueAsync()
  {
    var seed = new StatsSeeder(DbContext);
    SimpleDraft(seed, "Policy zero", policy: 0);
    SimpleDraft(seed, "Policy two released", policy: 2, release: _jan1);
    SimpleDraft(seed, "Policy two unreleased", policy: 2);
    SimpleDraft(seed, "Policy one", policy: 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync(includeAll: true);

    response.IncludesNonCanonical.Should().BeTrue();
    Total(response, "drafts").Should().Be(4);
  }

  [Fact]
  public async Task Handle_ShouldIgnoreNonMainFeedReleases_WhenResolvingPolicyTwoAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var part = seed.Draft("Patreon only", policy: 2).Part().Release("Patreon", _jan1);
    part.Pick("Heat", 1, 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    Total(await RecordBookAsync(), "drafts").Should().Be(0);
  }

  [Fact]
  public async Task Handle_ShouldBecomeCanonical_WhenAMainFeedReleaseArrivesAfterTheFactsAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var draft = seed.Draft("Late release", policy: 2);
    var part = draft.Part();
    part.Pick("Heat", 1, 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    Total(await RecordBookAsync(), "drafts").Should().Be(0);

    DbContext.DraftPartsReleases.Add(DraftPartRelease.Create(draft.Id, part.PublicId, MainFeed, _jan1));
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    await ClearRecordBookCacheAsync();

    Total(await RecordBookAsync(), "drafts").Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Totals and records on a hand-computed dataset
  // -------------------------------------------------------------------------

  /// <summary>
  /// Alpha (policy 0, episode 1), one part, 7 picks:
  ///   Heat   No.1 D1 landed
  ///   Alien  No.2 D2 vetoed by D1 (standing)
  ///   Brazil No.2 D2 vetoed by D1 (standing)           -> D2 has a 2-veto chain at No.2
  ///   Casino No.2 D2 vetoed by D1, overridden by D2    -> saved, does not extend the chain
  ///   Dune   No.3 D1 vetoed by the community, overridden by D2 -> saved
  ///   Elf    No.4 D2 self-vetoed (standing)
  ///   Fargo  No.5 D1 removed by commissioner override
  /// Bravo (policy 0, episode 2), clean: Heat No.1 D1, Alien No.2 D2.
  /// Charlie (policy 1) carries a veto and must stay out of the canonical scope.
  /// </summary>
  private async Task SeedBookAsync()
  {
    var seed = new StatsSeeder(DbContext);

    var alpha = seed.Draft("Alpha").Part(episode: 1, mainFeed: _jan1);
    alpha.Pick("Heat", 1, playedBy: 1);
    alpha.Pick("Alien", 2, playedBy: 2).Veto(DrafterKind, 1);
    alpha.Pick("Brazil", 2, playedBy: 2).Veto(DrafterKind, 1);
    alpha.Pick("Casino", 2, playedBy: 2).Veto(DrafterKind, 1, overridden: true, overriddenBy: 2);
    alpha.Pick("Dune", 3, playedBy: 1).Veto(CommunityKind, overridden: true, overriddenBy: 2);
    alpha.Pick("Elf", 4, playedBy: 2).Veto(DrafterKind, 2);
    alpha.Pick("Fargo", 5, playedBy: 1).Removed();

    var bravo = seed.Draft("Bravo").Part(episode: 2, mainFeed: _jan1.AddDays(7));
    bravo.Pick("Heat", 1, playedBy: 1);
    bravo.Pick("Alien", 2, playedBy: 2);

    var charlie = seed.Draft("Charlie", policy: 1).Part(episode: 3);
    charlie.Pick("Ghost", 1, playedBy: 3).Veto(DrafterKind, 1);

    seed
      .MovieHonorific("m_a", MovieHonorific.None, 1)
      .MovieHonorific("m_b", MovieHonorific.MarqueeOfFame, 2)
      .MovieHonorific("m_c", MovieHonorific.HatTrick, 3)
      .MovieHonorific("m_d", MovieHonorific.GrandSlam, 4)
      .MovieHonorific("m_e", MovieHonorific.HighFive, 5);

    await seed.SaveAsync(TestContext.Current.CancellationToken);
  }

  [Fact]
  public async Task Handle_ShouldComputeEveryTotal_FromTheSeededFactsAsync()
  {
    await SeedBookAsync();

    var response = await RecordBookAsync();

    Total(response, "drafts").Should().Be(2);
    Total(response, "picks-made").Should().Be(5); // Alpha: Heat, Casino, Dune. Bravo: Heat, Alien.
    Total(response, "unique-titles-drafted").Should().Be(4); // Heat, Casino, Dune, Alien
    Total(response, "vetoes-deployed").Should().Be(3); // Alien, Brazil, Elf stood
    Total(response, "vetoes-overridden").Should().Be(2); // Casino and the community veto on Dune
    Total(response, "commissioner-overrides").Should().Be(1);
    Total(response, "copacetic-drafts").Should().Be(1); // Bravo only
    Total(response, "self-vetoes").Should().Be(1);
    Total(response, "unique-guest-gms").Should().Be(2);
    Total(response, "marquee-of-fame-titles").Should().Be(4);
    Total(response, "hat-trick-titles").Should().Be(3);
    Total(response, "grand-slam-titles").Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldCountCommunityVetoes_ButNotAttributeThemToAnyDrafterAsync()
  {
    await SeedBookAsync();

    var response = await RecordBookAsync();

    // D1 issued 3 vetoes (Alien, Brazil, Casino); D2 issued 1 (Elf self-veto). The community veto is no one's.
    var mostUsed = Record(response, "guest-gm.most-vetoes-used");
    mostUsed!.Value.Should().Be(3);
    Holders(mostUsed).Should().Equal("Drafter 01");

    Total(response, "vetoes-overridden").Should().Be(2, "the community veto counts as a veto");
  }

  [Fact]
  public async Task Handle_ShouldMakeADraftNotCopacetic_WhenItHasAnOverriddenCommunityVetoAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var clean = seed.Draft("Clean").Part(episode: 1, mainFeed: _jan1);
    clean.Pick("Heat", 1, 1);
    var community = seed.Draft("Community").Part(episode: 2, mainFeed: _jan1.AddDays(1));
    community.Pick("Alien", 1, 1).Veto(CommunityKind, overridden: true, overriddenBy: 2);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    Total(response, "copacetic-drafts").Should().Be(1);
    Record(response, "draft.most-titles-drafted-in-a-copacetic-draft")!.Holders.Single().Name.Should().Be("Clean");
  }

  [Fact]
  public async Task Handle_ShouldMakeADraftNotCopacetic_WhenAPickWasRemovedByCommissionerAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var part = seed.Draft("Removed").Part(episode: 1, mainFeed: _jan1);
    part.Pick("Heat", 1, 1).Removed();
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    Total(response, "copacetic-drafts").Should().Be(0);
    Total(response, "commissioner-overrides").Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldRequireEveryPartToBeClean_WhenADraftHasSeveralPartsAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var draft = seed.Draft("Two parts", totalParts: 2);
    draft.Part(index: 1, episode: 1, mainFeed: _jan1).Pick("Heat", 1, 1);
    draft.Part(index: 2, episode: 1, mainFeed: _jan1.AddDays(7)).Pick("Alien", 1, 1).Veto(DrafterKind, 2);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    Total(response, "drafts").Should().Be(1);
    Total(response, "copacetic-drafts").Should().Be(0);
  }

  [Fact]
  public async Task Handle_ShouldRankRecordsEndToEndAsync()
  {
    await SeedBookAsync();

    var response = await RecordBookAsync();

    Holders(Record(response, "guest-gm.most-self-vetoes")).Should().Equal("Drafter 02");
    Holders(Record(response, "guest-gm.most-vetoes-overridden")).Should().Equal("Drafter 01");
    Holders(Record(response, "guest-gm.most-veto-overrides-deployed")).Should().Equal("Drafter 02");
    Record(response, "guest-gm.most-veto-overrides-deployed")!.Value.Should().Be(2);
    Holders(Record(response, "guest-gm.most-times-rescued-by-a-veto-override")).Should().Equal("Drafter 01", "Drafter 02");
    Holders(Record(response, "guest-gm.most-picks-removed-by-commissioner-override")).Should().Equal("Drafter 01");
    Holders(Record(response, "guest-gm.most-copacetic-drafts")).Should().Equal("Drafter 01", "Drafter 02");
    Record(response, "guest-gm.most-appearances")!.Value.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldRankDraftAndTitleRecordsEndToEndAsync()
  {
    await SeedBookAsync();

    var response = await RecordBookAsync();

    var vetoed = Record(response, "draft.most-picks-vetoed");
    vetoed!.Value.Should().Be(3); // Alien, Brazil, Elf
    vetoed.Holders.Single().Name.Should().Be("Alpha");

    Record(response, "draft.most-commissioner-overrides")!.Holders.Single().Name.Should().Be("Alpha");
    Record(response, "draft.most-titles-drafted")!.Holders.Select(h => h.Name).Should().Equal("Alpha");

    var title = Record(response, "title.most-times-drafted");
    title!.Value.Should().Be(2);
    Holders(title).Should().Equal("Heat");

    Record(response, "title.most-times-drafted-no1")!.Holders.Single().Name.Should().Be("Heat");
  }

  [Fact]
  public async Task Handle_ShouldBuildAConsecutiveVetoChain_ThatOverriddenVetoesDoNotExtendAsync()
  {
    await SeedBookAsync();

    var response = await RecordBookAsync();

    // D2 at No.2: Alien and Brazil stood, Casino was saved. The chain is 2, not 3.
    var atOnePick = Record(response, "guest-gm.most-times-vetoed-at-a-single-pick");
    atOnePick!.Value.Should().Be(2);
    atOnePick.Holders.Single().Name.Should().Be("Drafter 02");

    Record(response, "guest-gm.most-times-with-consecutive-vetoes-at-a-single-pick")!.Value.Should().Be(1);
    Record(response, "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick").Should().BeNull();
  }

  [Fact]
  public async Task Handle_ShouldCountAThreeVetoChainForThreeOrMoreAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var part = seed.Draft("Chain").Part(episode: 1, mainFeed: _jan1);
    part.Pick("One", 4, 2).Veto(DrafterKind, 1);
    part.Pick("Two", 4, 2).Veto(DrafterKind, 1);
    part.Pick("Three", 4, 2).Veto(DrafterKind, 1);
    part.Pick("Four", 4, 2);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    Record(response, "guest-gm.most-times-vetoed-at-a-single-pick")!.Value.Should().Be(3);
    Record(response, "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick")!.Value.Should().Be(1);
    Record(response, "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick-in-a-single-draft")!.Value.Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldEmitTieredRecords_OnlyForDraftersWithEnoughAppearancesAsync()
  {
    var seed = new StatsSeeder(DbContext);

    // Drafter 3 appears in 5 drafts (one veto against them). Drafter 4 appears in 2.
    for (var i = 1; i <= 5; i++)
    {
      var part = seed.Draft($"Five {i}").Part(episode: i, mainFeed: _jan1.AddDays(i));
      var pick = part.Pick($"Five title {i}", 1, 3);

      if (i == 1)
      {
        pick.Veto(DrafterKind, 4);
      }
    }

    for (var i = 1; i <= 2; i++)
    {
      seed.Draft($"Two {i}").Part(episode: 10 + i, mainFeed: _jan1.AddDays(20 + i)).Pick($"Two title {i}", 1, 4);
    }

    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    var fewest = Record(response, "guest-gm.fewest-picks-vetoed.min5");
    fewest.Should().NotBeNull();
    Holders(fewest).Should().Equal("Drafter 03");
    fewest.Qualifier.Should().Be("5+ drafts");
    fewest.Value.Should().Be(1);
    Record(response, "guest-gm.fewest-picks-vetoed.min10").Should().BeNull();

    var pct = Record(response, "guest-gm.highest-pct-without-being-vetoed.min5");
    pct!.Value.Should().Be(80m);
  }

  [Fact]
  public async Task Handle_ShouldCountADrafterOncePerDraft_WhenTheirDraftHasSeveralPartsAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var draft = seed.Draft("Long one", totalParts: 3);

    for (var index = 1; index <= 3; index++)
    {
      draft.Part(index, episode: 1, mainFeed: _jan1.AddDays(index)).Pick($"Part {index} title", 1, 1);
    }

    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    Record(response, "guest-gm.most-appearances")!.Value.Should().Be(1);
    Total(response, "drafts").Should().Be(1);
    Total(response, "picks-made").Should().Be(3);
  }

  [Fact]
  public async Task Handle_ShouldCreditEachTeamMember_ButCountTheTeamPickOnceAtDraftLevelAsync()
  {
    var seed = new StatsSeeder(DbContext);
    var part = seed.Draft("Teams").Part(episode: 1, mainFeed: _jan1);
    part.TeamPick("Heat", 1, team: 1, members: [1, 2]);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    Total(response, "picks-made").Should().Be(1);
    Total(response, "unique-guest-gms").Should().Be(2);
    Record(response, "draft.most-titles-drafted")!.Value.Should().Be(1);
    Record(response, "guest-gm.most-titles-drafted")!.Holders.Select(h => h.Name).Should().Equal("Drafter 01", "Drafter 02");
  }

  [Fact]
  public async Task Handle_ShouldReadEpisodeNumbersFromDraftSummariesAsync()
  {
    var seed = new StatsSeeder(DbContext);

    // Drafter 1 appears in episodes 1, 3 and 2. A commissioner override at episode 2 breaks the run.
    seed.Draft("E3").Part(episode: 3, mainFeed: _jan1.AddDays(2)).Pick("T3", 1, 1);
    seed.Draft("E1").Part(episode: 1, mainFeed: _jan1).Pick("T1", 1, 1);
    seed.Draft("E2").Part(episode: 2, mainFeed: _jan1.AddDays(1)).Pick("T2", 1, 1).Removed();
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    var response = await RecordBookAsync();

    // Episode order: 1 clean, 2 overridden (break), 3 clean => longest run is 1.
    Record(response, "guest-gm.most-appearances-without-a-commissioner-override")!.Value.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Cache
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldServeTheSecondCallFromCache_AndEqualTheFirstAsync()
  {
    await SeedBookAsync();

    var first = await RecordBookAsync();

    // Change the facts. A cached response must not notice.
    var extra = new StatsSeeder(DbContext, idOffset: 100);
    extra.Draft("Extra").Part(episode: 9, mainFeed: _jan1.AddDays(30)).Pick("Extra", 1, 5);
    await extra.SaveAsync(TestContext.Current.CancellationToken);

    var second = await RecordBookAsync();

    second.Should().BeEquivalentTo(first, o => o.WithStrictOrdering());
    Total(second, "drafts").Should().Be(2);
    second.GeneratedAtUtc.Should().Be(first.GeneratedAtUtc);
  }

  [Fact]
  public async Task Handle_ShouldKeepSeparateCacheEntriesPerScopeAsync()
  {
    await SeedBookAsync();

    var canonical = await RecordBookAsync();
    var all = await RecordBookAsync(includeAll: true);
    var canonicalAgain = await RecordBookAsync();
    var allAgain = await RecordBookAsync(includeAll: true);

    Total(canonical, "drafts").Should().Be(2);
    Total(all, "drafts").Should().Be(3);
    canonicalAgain.IncludesNonCanonical.Should().BeFalse();
    allAgain.IncludesNonCanonical.Should().BeTrue();
    allAgain.Should().BeEquivalentTo(all, o => o.WithStrictOrdering());
    canonicalAgain.Should().BeEquivalentTo(canonical, o => o.WithStrictOrdering());
  }

  [Fact]
  public async Task Handle_ShouldReturnAnEmptyBook_WhenThereAreNoFactsAsync()
  {
    var response = await RecordBookAsync();

    response.Totals.Should().HaveCount(12).And.OnlyContain(t => t.Value == 0);
    response.Sections.Should().HaveCount(3);
  }
}

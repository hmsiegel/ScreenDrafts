using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.RecordBookTestData;

namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class DrafterRecordsCalculatorTests
{
  private static readonly IReadOnlySet<Guid> _noCopacetic = new HashSet<Guid>();

  private static RecordBookSection Build(
    IEnumerable<DrafterDraftRow> rows,
    IEnumerable<PickSlotRow>? slots = null,
    IReadOnlySet<Guid>? copacetic = null) =>
    DrafterRecordsCalculator.Build([.. rows], [.. slots ?? []], copacetic ?? _noCopacetic);

  private static IEnumerable<string> HolderNames(RecordItem? item) =>
    item?.Holders.Select(h => h.Name) ?? [];

  // -------------------------------------------------------------------------
  // Appearances
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldCountAppearancesOncePerDraft_WhenDrafterHasOneRowPerDraft()
  {
    var section = Build(Appearances(1, 3));

    Find(section, "guest-gm.most-appearances")!.Value.Should().Be(3);
  }

  [Fact]
  public void Build_ShouldIgnoreRowsWhereDrafterDidNotAppear()
  {
    var rows = Appearances(1, 2).Concat([DrafterDraft(1, 3, r => r.Appeared = false)]);

    Find(Build(rows), "guest-gm.most-appearances")!.Value.Should().Be(2);
  }

  [Fact]
  public void Build_ShouldOmitDrafterWithNoAppearancesAtAll()
  {
    var rows = new[] { DrafterDraft(1, 1, r => { r.Appeared = false; r.VetoesUsed = 9; }) };

    Find(Build(rows), "guest-gm.most-vetoes-used").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldReturnEveryHolderSortedByName_WhenAppearancesTie()
  {
    var rows = Appearances(2, 4).Concat(Appearances(1, 4)).Concat(Appearances(3, 2));

    var item = Find(Build(rows), "guest-gm.most-appearances");

    item!.Value.Should().Be(4);
    HolderNames(item).Should().Equal("Drafter 1", "Drafter 2");
  }

  [Fact]
  public void Build_ShouldCountDraftsWithATitleVetoed_OncePerDraft()
  {
    var rows = Appearances(1, 4, (draft, r) => r.PicksVetoed = draft <= 2 ? 3 : 0);

    Find(Build(rows), "guest-gm.most-drafts-with-a-title-vetoed")!.Value.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Tiers
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldExcludeDrafterWithNineDrafts_FromTenPlusRecords()
  {
    var section = Build(Appearances(1, 9));

    Find(section, "guest-gm.highest-pct-without-being-vetoed.min5").Should().NotBeNull();
    Find(section, "guest-gm.highest-pct-without-being-vetoed.min10").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldIncludeDrafterWithTenDrafts_InTenPlusRecords()
  {
    var section = Build(Appearances(1, 10));

    var item = Find(section, "guest-gm.highest-pct-without-being-vetoed.min10");

    item!.Qualifier.Should().Be("10+ drafts");
    item.Holders.Single().Context.Should().Be("10 drafts");
  }

  [Fact]
  public void Build_ShouldUseEachRecordsOwnTierList()
  {
    var section = Build(Appearances(1, 20));

    Codes(section, "guest-gm.most-vetoes-used-per-draft").Should().BeEmpty(); // zero vetoes: a "most" record is absent
    Codes(section, "guest-gm.fewest-vetoes-used").Should().Equal(
      "guest-gm.fewest-vetoes-used.min5",
      "guest-gm.fewest-vetoes-used.min10",
      "guest-gm.fewest-vetoes-used.min15",
      "guest-gm.fewest-vetoes-used.min20");
    Codes(section, "guest-gm.highest-pct-copacetic-drafts").Should().BeEmpty(); // no copacetic drafts
  }

  [Fact]
  public void Build_ShouldEmitMostVetoesPerDraftForTiersFiveToFifteenOnly()
  {
    var rows = Appearances(1, 20, (_, r) => r.VetoesUsed = 1);

    Codes(Build(rows), "guest-gm.most-vetoes-used-per-draft").Should().Equal(
      "guest-gm.most-vetoes-used-per-draft.min5",
      "guest-gm.most-vetoes-used-per-draft.min10",
      "guest-gm.most-vetoes-used-per-draft.min15");
  }

  [Fact]
  public void Build_ShouldEmitCopaceticPercentageOnlyFromTenDrafts()
  {
    var rows = Appearances(1, 20);
    var copacetic = rows.Select(r => r.DraftId).ToHashSet();

    Codes(Build(rows, copacetic: copacetic), "guest-gm.highest-pct-copacetic-drafts").Should().Equal(
      "guest-gm.highest-pct-copacetic-drafts.min10",
      "guest-gm.highest-pct-copacetic-drafts.min15",
      "guest-gm.highest-pct-copacetic-drafts.min20");
  }

  // -------------------------------------------------------------------------
  // Percentages and ratios
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldComputePercentageWithoutBeingVetoed()
  {
    var rows = Appearances(1, 10, (draft, r) => r.PicksVetoed = draft <= 2 ? 1 : 0);

    var item = Find(Build(rows), "guest-gm.highest-pct-without-being-vetoed.min10");

    item!.Value.Should().Be(80m);
    item.Format.Should().Be(RecordRanker.Percent);
  }

  [Fact]
  public void Build_ShouldComputeVetoesUsedPerDraft()
  {
    var rows = Appearances(1, 5, (draft, r) => r.VetoesUsed = draft == 1 ? 3 : 0);

    var item = Find(Build(rows), "guest-gm.most-vetoes-used-per-draft.min5");

    item!.Value.Should().Be(0.6m);
    item.Format.Should().Be(RecordRanker.Ratio);
  }

  [Fact]
  public void Build_ShouldComputePicksVetoedPerDraft()
  {
    var rows = Appearances(1, 5, (_, r) => r.PicksVetoed = 1)
      .Concat(Appearances(2, 5, (draft, r) => r.PicksVetoed = draft <= 2 ? 1 : 0));

    var most = Find(Build(rows), "guest-gm.most-picks-vetoed-per-draft.min5");
    var fewest = Find(Build(rows), "guest-gm.fewest-picks-vetoed-per-draft.min5");

    most!.Value.Should().Be(1m);
    HolderNames(most).Should().Equal("Drafter 1");
    fewest!.Value.Should().Be(0.4m);
    HolderNames(fewest).Should().Equal("Drafter 2");
  }

  [Fact]
  public void Build_ShouldComputeCopaceticPercentage()
  {
    var rows = Appearances(1, 10);
    var copacetic = rows.Take(3).Select(r => r.DraftId).ToHashSet();

    var item = Find(Build(rows, copacetic: copacetic), "guest-gm.highest-pct-copacetic-drafts.min10");

    item!.Value.Should().Be(30m);
  }

  // -------------------------------------------------------------------------
  // Picks and single-draft records
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldSumTitlesDraftedAcrossDrafts()
  {
    var rows = Appearances(1, 3, (_, r) => r.PicksLanded = 4).Concat(Appearances(2, 2, (_, r) => r.PicksLanded = 5));

    var item = Find(Build(rows), "guest-gm.most-titles-drafted");

    item!.Value.Should().Be(12);
    HolderNames(item).Should().Equal("Drafter 1");
  }

  [Fact]
  public void Build_ShouldNameTheDraft_ForSingleDraftRecords()
  {
    var rows = new[]
    {
      DrafterDraft(1, 1, r => r.PicksVetoed = 1),
      DrafterDraft(1, 2, r => r.PicksVetoed = 3),
      DrafterDraft(2, 3, r => r.PicksVetoed = 2),
    };

    var item = Find(Build(rows), "guest-gm.most-picks-vetoed-single-draft");

    item!.Value.Should().Be(3);
    item.Holders.Single().Name.Should().Be("Drafter 1");
    item.Holders.Single().Context.Should().Be("Draft 2");
  }

  [Fact]
  public void Build_ShouldReportPersonPublicIdAsHolderId()
  {
    var item = Find(Build(Appearances(7, 1)), "guest-gm.most-appearances");

    item!.Holders.Single().PublicId.Should().Be("p_7");
    item.Holders.Single().Kind.Should().Be("drafter");
  }

  // -------------------------------------------------------------------------
  // Commissioner overrides
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldOrderLongestRunByEpisodeNumber_AndSortNullEpisodesLast()
  {
    // Episodes 1, 2 clean; 3 overridden; 4 and "no episode" clean.
    // Episode order gives runs 2 then 2 (the null-episode draft sorts last). If null sorted first it would be 3.
    var rows = new[]
    {
      DrafterDraft(1, 4, r => r.EpisodeNumber = 4),
      DrafterDraft(1, 1, r => r.EpisodeNumber = 1),
      DrafterDraft(1, 3, r => { r.EpisodeNumber = 3; r.PicksRemovedByCommissioner = 1; }),
      DrafterDraft(1, 5, r => r.EpisodeNumber = null),
      DrafterDraft(1, 2, r => r.EpisodeNumber = 2),
    };

    var item = Find(Build(rows), "guest-gm.most-appearances-without-a-commissioner-override");

    item!.Value.Should().Be(2);
  }

  [Fact]
  public void Build_ShouldBreakLongestRun_AtACommissionerOverriddenAppearance()
  {
    var rows = Appearances(1, 7, (draft, r) => r.PicksRemovedByCommissioner = draft == 4 ? 1 : 0);

    // 1,2,3 | 4 breaks | 5,6,7
    Find(Build(rows), "guest-gm.most-appearances-without-a-commissioner-override")!.Value.Should().Be(3);
  }

  [Fact]
  public void Build_ShouldCountWholeRun_WhenNoCommissionerOverrides()
  {
    var rows = Appearances(1, 5);

    Find(Build(rows), "guest-gm.most-appearances-without-a-commissioner-override")!.Value.Should().Be(5);
  }

  [Fact]
  public void Build_ShouldRankPicksRemovedByCommissionerOverride()
  {
    var rows = new[]
    {
      DrafterDraft(1, 1, r => r.PicksRemovedByCommissioner = 2),
      DrafterDraft(1, 2, r => r.PicksRemovedByCommissioner = 1),
      DrafterDraft(2, 3, r => r.PicksRemovedByCommissioner = 2),
    };

    var career = Find(Build(rows), "guest-gm.most-picks-removed-by-commissioner-override");
    var single = Find(Build(rows), "guest-gm.most-picks-removed-by-commissioner-override-single-draft");

    career!.Value.Should().Be(3);
    HolderNames(career).Should().Equal("Drafter 1");
    single!.Value.Should().Be(2);
    HolderNames(single).Should().Equal("Drafter 1", "Drafter 2");
  }

  // -------------------------------------------------------------------------
  // Vetoes and veto overrides
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldRankVetoRecords()
  {
    var rows = new[]
    {
      DrafterDraft(1, 1, r => { r.VetoesUsed = 4; r.SelfVetoes = 1; r.No1VetoesUsed = 1; }),
      DrafterDraft(2, 2, r => { r.VetoesUsed = 2; r.SelfVetoes = 3; r.No1VetoesUsed = 1; }),
    };

    var section = Build(rows);

    HolderNames(Find(section, "guest-gm.most-vetoes-used")).Should().Equal("Drafter 1");
    HolderNames(Find(section, "guest-gm.most-self-vetoes")).Should().Equal("Drafter 2");
    HolderNames(Find(section, "guest-gm.most-times-vetoing-a-no1-pick")).Should().Equal("Drafter 1", "Drafter 2");
  }

  [Fact]
  public void Build_ShouldRankVetoOverrideRecords()
  {
    var rows = new[]
    {
      DrafterDraft(1, 1, r => { r.VetoesOverridden = 2; r.OverridesDeployed = 0; r.PicksSaved = 1; }),
      DrafterDraft(2, 2, r => { r.VetoesOverridden = 1; r.OverridesDeployed = 3; r.PicksSaved = 0; }),
    };

    var section = Build(rows);

    HolderNames(Find(section, "guest-gm.most-vetoes-overridden")).Should().Equal("Drafter 1");
    HolderNames(Find(section, "guest-gm.most-veto-overrides-deployed")).Should().Equal("Drafter 2");
    HolderNames(Find(section, "guest-gm.most-times-rescued-by-a-veto-override")).Should().Equal("Drafter 1");
  }

  // -------------------------------------------------------------------------
  // Consecutive veto chains
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldCountChainOfThree_ForThreeOrMoreAndForTwoOrMore()
  {
    var slots = new[] { Slot(1, 1, position: 5, timesVetoed: 3) };

    var section = Build([DrafterDraft(1, 1)], slots);

    Find(section, "guest-gm.most-times-vetoed-at-a-single-pick")!.Value.Should().Be(3);
    Find(section, "guest-gm.most-times-with-consecutive-vetoes-at-a-single-pick")!.Value.Should().Be(1);
    Find(section, "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick")!.Value.Should().Be(1);
    Find(section, "guest-gm.most-times-vetoed-consecutively-in-a-single-draft")!.Value.Should().Be(1);
    Find(section, "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick-in-a-single-draft")!.Value.Should().Be(1);
  }

  [Fact]
  public void Build_ShouldNotCountSingleVetoAsConsecutive()
  {
    var slots = new[] { Slot(1, 1, position: 5, timesVetoed: 1) };

    var section = Build([DrafterDraft(1, 1)], slots);

    Find(section, "guest-gm.most-times-vetoed-at-a-single-pick")!.Value.Should().Be(1);
    Find(section, "guest-gm.most-times-with-consecutive-vetoes-at-a-single-pick").Should().BeNull();
    Find(section, "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldCountChainOfTwo_ForTwoOrMoreButNotThreeOrMore()
  {
    var slots = new[] { Slot(1, 1, position: 5, timesVetoed: 2) };

    var section = Build([DrafterDraft(1, 1)], slots);

    Find(section, "guest-gm.most-times-with-consecutive-vetoes-at-a-single-pick")!.Value.Should().Be(1);
    Find(section, "guest-gm.most-times-vetoed-3-or-more-times-at-a-single-pick").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldSumChainsAcrossDrafts_ForCareerButSplitThemPerDraft()
  {
    var slots = new[]
    {
      Slot(1, 1, position: 5, timesVetoed: 2),
      Slot(1, 1, position: 9, timesVetoed: 2),
      Slot(1, 2, position: 3, timesVetoed: 2),
    };

    var section = Build([DrafterDraft(1, 1), DrafterDraft(1, 2)], slots);

    Find(section, "guest-gm.most-times-with-consecutive-vetoes-at-a-single-pick")!.Value.Should().Be(3);

    var perDraft = Find(section, "guest-gm.most-times-vetoed-consecutively-in-a-single-draft");
    perDraft!.Value.Should().Be(2);
    perDraft.Holders.Single().Context.Should().Be("Draft 1");
  }

  [Fact]
  public void Build_ShouldNameThePosition_ForMostTimesVetoedAtASinglePick()
  {
    var slots = new[] { Slot(1, 1, position: 12, timesVetoed: 4), Slot(2, 1, position: 2, timesVetoed: 1) };

    var item = Find(Build([DrafterDraft(1, 1), DrafterDraft(2, 1)], slots), "guest-gm.most-times-vetoed-at-a-single-pick");

    item!.Holders.Single().Context.Should().Be("No. 12, Draft 1");
  }

  // -------------------------------------------------------------------------
  // Copacetic
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldCountCopaceticDraftsOnlyWhereTheDrafterAppeared()
  {
    var rows = Appearances(1, 3).Concat([DrafterDraft(1, 4, r => r.Appeared = false)]).ToList();
    var copacetic = new HashSet<Guid> { Id(1), Id(2), Id(4) };

    var item = Find(Build(rows, copacetic: copacetic), "guest-gm.most-copacetic-drafts");

    item!.Value.Should().Be(2);
  }

  [Fact]
  public void Build_ShouldListOnlyDraftersWithNoCopaceticDraft_ForMostDraftsWithoutOne()
  {
    var rows = Appearances(1, 6).Concat(Appearances(2, 3)).ToList();
    var copacetic = new HashSet<Guid> { Id(6) };

    var item = Find(Build(rows, copacetic: copacetic), "guest-gm.most-drafts-without-a-copacetic-draft");

    // Drafter 1 appeared in copacetic draft 6, so only Drafter 2 qualifies (3 drafts).
    item!.Value.Should().Be(3);
    HolderNames(item).Should().Equal("Drafter 2");
  }

  // -------------------------------------------------------------------------
  // Shape
  // -------------------------------------------------------------------------

  [Fact]
  public void Build_ShouldReturnTheGuestGmSectionWithAllGroups()
  {
    var section = Build([]);

    section.Key.Should().Be("guest-gm");
    section.Groups.Select(g => g.Key).Should().Equal(
      "appearances",
      "picks",
      "vetoes",
      "consecutive-vetoes",
      "veto-overrides",
      "commissioner-overrides",
      "copacetic");
    section.Groups.SelectMany(g => g.Records).Should().BeEmpty();
  }

  [Fact]
  public void Build_ShouldThrow_WhenArgumentsAreNull()
  {
    FluentActions.Invoking(() => DrafterRecordsCalculator.Build(null!, [], _noCopacetic)).Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => DrafterRecordsCalculator.Build([], null!, _noCopacetic)).Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => DrafterRecordsCalculator.Build([], [], null!)).Should().Throw<ArgumentNullException>();
  }

  private static IEnumerable<string> Codes(RecordBookSection section, string prefix) =>
    section.Groups.SelectMany(g => g.Records).Select(r => r.Code).Where(c => c.StartsWith(prefix + ".min", StringComparison.Ordinal));
}

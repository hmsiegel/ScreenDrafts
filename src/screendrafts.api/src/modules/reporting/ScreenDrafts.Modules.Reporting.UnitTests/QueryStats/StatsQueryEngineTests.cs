using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.QueryDataBuilder;

namespace ScreenDrafts.Modules.Reporting.UnitTests.QueryStats;

public sealed class StatsQueryEngineTests
{
  private static StatsQueryResult Run(
    QueryDataBuilder data,
    string metric,
    string groupBy,
    Action<SpecOptions>? configure = null) =>
    StatsQueryEngine.Run(data.Build(), Spec(metric, groupBy, configure));

  private static Dictionary<string, decimal> Values(StatsQueryResult result) =>
    result.Rows.ToDictionary(r => r.Name, r => r.Value);

  /// <summary>Two drafts, every pick outcome, and one veto of each issuer kind. Used to exercise every pairing.</summary>
  private static QueryDataBuilder Rich()
  {
    var b = new QueryDataBuilder().Draft(1, "Main", "Standard", 1).Draft(2, "Bonus", "Mega", 2);

    var landed = b.Pick(1, "A");
    var vetoed = b.Pick(1, "B", PickOutcome.Vetoed);
    var saved = b.Pick(1, "C", PickOutcome.Saved);
    var removed = b.Pick(2, "D", PickOutcome.Removed);
    var other = b.Pick(2, "A");

    b.Credit(landed, 1).Credit(vetoed, 2).Credit(saved, 1, 3).Credit(removed, 2).Credit(other, 3);
    b.Veto(vetoed, DrafterKind, 1)
      .Veto(saved, DrafterKind, 2, overridden: true)
      .Veto(vetoed, TeamKind, 1)
      .Veto(removed, CommunityKind, 1)
      .Veto(landed, DrafterKind, 1, self: true);

    return b;
  }

  public static TheoryData<string, string> AllowedPairs
  {
    get
    {
      var data = new TheoryData<string, string>();

      foreach (var metric in StatsMetrics.All)
      {
        foreach (var groupBy in metric.GroupBys)
        {
          data.Add(metric.Code, groupBy);
        }
      }

      return data;
    }
  }

  [Theory]
  [MemberData(nameof(AllowedPairs))]
  public void Run_ShouldProduceRows_ForEveryMetricAndAllowedGroupBy(string metric, string groupBy)
  {
    var result = Run(Rich(), metric, groupBy, o => o.Ascending = true);

    result.Rows.Should().NotBeEmpty();
    result.TotalGroups.Should().Be(result.Rows.Count);
    result.Rows.Should().OnlyContain(r => r.Rank >= 1);
  }

  // -------------------------------------------------------------------------
  // Pick metrics
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldCountLandedPicksPerDrafter_ForTitlesDrafted()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1)
      .Credit(b.Pick(1, "B"), 1)
      .Credit(b.Pick(1, "C", PickOutcome.Vetoed), 1)
      .Credit(b.Pick(1, "D", PickOutcome.Saved), 2)
      .Credit(b.Pick(1, "E", PickOutcome.Removed), 2);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    Values(result).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 2, ["Drafter 2"] = 1 });
    result.Rows[0].Context.Should().Be("1 draft");
    result.Rows[0].PublicId.Should().Be("p_1");
  }

  [Fact]
  public void Run_ShouldGroupTitlesDraftedBySeriesDraftTypeDraftAndTitle()
  {
    var b = new QueryDataBuilder().Draft(1, "Main", "Standard", 1).Draft(2, "Bonus", "Mega", 2);
    b.Credit(b.Pick(1, "A"), 1);
    b.Credit(b.Pick(1, "B"), 1);
    b.Credit(b.Pick(2, "A"), 1);

    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Series)).Should().Equal(new Dictionary<string, decimal> { ["Main"] = 2, ["Bonus"] = 1 });
    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.DraftType)).Should().Equal(new Dictionary<string, decimal> { ["Standard"] = 2, ["Mega"] = 1 });
    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Draft)).Should().Equal(new Dictionary<string, decimal> { ["Draft 1"] = 2, ["Draft 2"] = 1 });
    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Title)).Should().Equal(new Dictionary<string, decimal> { ["A"] = 2, ["B"] = 1 });
  }

  [Fact]
  public void Run_ShouldCountPicksVetoedAndCommissionerOverridesByCredit()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A", PickOutcome.Vetoed), 1)
      .Credit(b.Pick(1, "B", PickOutcome.Saved), 1)
      .Credit(b.Pick(1, "C", PickOutcome.Removed), 2);

    Values(Run(b, StatsMetrics.PicksVetoed, StatsGroupBys.Drafter)).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 1 });
    Values(Run(b, StatsMetrics.CommissionerOverrides, StatsGroupBys.Drafter)).Should().Equal(new Dictionary<string, decimal> { ["Drafter 2"] = 1 });
    Values(Run(b, StatsMetrics.CommissionerOverrides, StatsGroupBys.Title)).Should().Equal(new Dictionary<string, decimal> { ["C"] = 1 });
  }

  [Fact]
  public void Run_ShouldNotDoubleCount_WhenATeamPickIsCreditedToTwoDrafters()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1, 2);

    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter)).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 1, ["Drafter 2"] = 1 });
    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Draft)).Should().Equal(new Dictionary<string, decimal> { ["Draft 1"] = 1 });
    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Series)).Should().Equal(new Dictionary<string, decimal> { ["Main"] = 1 });
    Values(Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Title)).Should().Equal(new Dictionary<string, decimal> { ["A"] = 1 });
  }

  // -------------------------------------------------------------------------
  // Veto metrics
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldAttributeVetoesOnlyToDrafterIssuers_WhenGroupedByDrafter()
  {
    var b = new QueryDataBuilder();
    var pick = b.Pick(1, "A", PickOutcome.Vetoed);
    b.Credit(pick, 1, 2);
    b.Veto(pick, DrafterKind, 1)
      .Veto(pick, DrafterKind, 1, overridden: true)
      .Veto(pick, TeamKind, 1)
      .Veto(pick, CommunityKind, 1);

    Values(Run(b, StatsMetrics.VetoesUsed, StatsGroupBys.Drafter)).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 2 });
  }

  [Fact]
  public void Run_ShouldCountEveryVeto_WhenGroupedByDraftOrSeries()
  {
    var b = new QueryDataBuilder();
    var pick = b.Pick(1, "A", PickOutcome.Vetoed);
    b.Credit(pick, 1);
    b.Veto(pick, DrafterKind, 1).Veto(pick, TeamKind, 2).Veto(pick, CommunityKind, 3, overridden: true);

    Values(Run(b, StatsMetrics.VetoesUsed, StatsGroupBys.Draft)).Should().Equal(new Dictionary<string, decimal> { ["Draft 1"] = 3 });
    Values(Run(b, StatsMetrics.VetoesUsed, StatsGroupBys.Series)).Should().Equal(new Dictionary<string, decimal> { ["Main"] = 3 });
    Values(Run(b, StatsMetrics.VetoesUsed, StatsGroupBys.Title)).Should().Equal(new Dictionary<string, decimal> { ["A"] = 3 });
  }

  [Fact]
  public void Run_ShouldCountOverriddenVetoesByIssuingDrafter()
  {
    var b = new QueryDataBuilder();
    var pick = b.Pick(1, "A", PickOutcome.Saved);
    b.Credit(pick, 1, 2);
    b.Veto(pick, DrafterKind, 1, overridden: true)
      .Veto(pick, DrafterKind, 2)
      .Veto(pick, CommunityKind, 1, overridden: true);

    Values(Run(b, StatsMetrics.VetoesOverridden, StatsGroupBys.Drafter)).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 1 });
    Values(Run(b, StatsMetrics.VetoesOverridden, StatsGroupBys.Draft)).Should().Equal(new Dictionary<string, decimal> { ["Draft 1"] = 2 });
  }

  [Fact]
  public void Run_ShouldExcludeSelfVetoes_WhenTheVetoWasOverridden()
  {
    var b = new QueryDataBuilder();
    var standing = b.Pick(1, "A", PickOutcome.Vetoed);
    var overridden = b.Pick(1, "B", PickOutcome.Saved);
    b.Credit(standing, 1).Credit(overridden, 1);
    b.Veto(standing, DrafterKind, 1, self: true).Veto(overridden, DrafterKind, 1, overridden: true, self: true);

    Values(Run(b, StatsMetrics.SelfVetoes, StatsGroupBys.Drafter)).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 1 });
    Values(Run(b, StatsMetrics.SelfVetoes, StatsGroupBys.Draft)).Should().Equal(new Dictionary<string, decimal> { ["Draft 1"] = 1 });
  }

  [Fact]
  public void Run_ShouldNotAttributeTeamSelfVetoesToADrafter()
  {
    var b = new QueryDataBuilder();
    var pick = b.Pick(1, "A", PickOutcome.Vetoed);
    b.Credit(pick, 1, 2);
    b.Veto(pick, TeamKind, 1, self: true);

    Run(b, StatsMetrics.SelfVetoes, StatsGroupBys.Drafter).Rows.Should().BeEmpty();
    Values(Run(b, StatsMetrics.SelfVetoes, StatsGroupBys.Draft)).Should().Equal(new Dictionary<string, decimal> { ["Draft 1"] = 1 });
  }

  // -------------------------------------------------------------------------
  // Appearances, copacetic, averages
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldCountAppearancesOncePerDraft_ForADrafter()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(1, "B"), 1).Credit(b.Pick(2, "C"), 1).Credit(b.Pick(2, "D"), 2);

    Values(Run(b, StatsMetrics.Appearances, StatsGroupBys.Drafter)).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 2, ["Drafter 2"] = 1 });
  }

  [Fact]
  public void Run_ShouldCountDistinctDrafterDraftPairs_ForAppearancesBySeries()
  {
    var b = new QueryDataBuilder().Draft(1, "Main").Draft(2, "Bonus");
    b.Credit(b.Pick(1, "A"), 1, 2).Credit(b.Pick(1, "B"), 1).Credit(b.Pick(2, "C"), 1);

    Values(Run(b, StatsMetrics.Appearances, StatsGroupBys.Series)).Should().Equal(new Dictionary<string, decimal> { ["Main"] = 2, ["Bonus"] = 1 });
  }

  private static QueryDataBuilder CopaceticData()
  {
    var b = new QueryDataBuilder().Draft(1, "Main").Draft(2, "Main").Draft(3, "Bonus").Draft(4, "Bonus");
    var clean = b.Pick(1, "A");
    var communityVeto = b.Pick(2, "B", PickOutcome.Saved);
    var removed = b.Pick(3, "C", PickOutcome.Removed);
    var clean2 = b.Pick(4, "D");

    b.Credit(clean, 1, 2).Credit(communityVeto, 1).Credit(removed, 1).Credit(clean2, 1);
    b.Veto(communityVeto, CommunityKind, 1, overridden: true);

    return b;
  }

  [Fact]
  public void Run_ShouldCountCopaceticDraftsPerDrafter_ExcludingDraftsWithAnyVetoOrOverride()
  {
    var result = Run(CopaceticData(), StatsMetrics.CopaceticDrafts, StatsGroupBys.Drafter);

    // Draft 2 has a (community, overridden) veto and draft 3 a commissioner removal: only 1 and 4 are clean.
    Values(result).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 2, ["Drafter 2"] = 1 });
  }

  [Fact]
  public void Run_ShouldCountCopaceticDraftsBySeries()
  {
    var result = Run(CopaceticData(), StatsMetrics.CopaceticDrafts, StatsGroupBys.Series);

    Values(result).Should().Equal(new Dictionary<string, decimal> { ["Main"] = 1, ["Bonus"] = 1 });
  }

  [Fact]
  public void Run_ShouldComputeCopaceticOverTheFilteredDraftsOnly()
  {
    var result = Run(CopaceticData(), StatsMetrics.CopaceticDrafts, StatsGroupBys.Drafter, o => o.Series.Add("Main"));

    Values(result).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 1, ["Drafter 2"] = 1 });
  }

  [Fact]
  public void Run_ShouldDivideVetoesIssuedByAppearances_ForAvgVetoesPerDraftByDrafter()
  {
    var b = new QueryDataBuilder();
    var p1 = b.Pick(1, "A", PickOutcome.Vetoed);
    var p2 = b.Pick(2, "B", PickOutcome.Vetoed);
    b.Credit(p1, 1).Credit(p2, 1);
    b.Veto(p1, DrafterKind, 1).Veto(p2, DrafterKind, 1).Veto(p2, DrafterKind, 1, overridden: true).Veto(p1, CommunityKind, 1);

    var result = Run(b, StatsMetrics.AvgVetoesPerDraft, StatsGroupBys.Drafter);

    Values(result).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 1.5m });
  }

  [Fact]
  public void Run_ShouldDivideAllVetoesByDistinctDrafts_ForAvgVetoesPerDraftBySeries()
  {
    var b = new QueryDataBuilder().Draft(1, "Main").Draft(2, "Main");
    var p1 = b.Pick(1, "A", PickOutcome.Vetoed);
    var p2 = b.Pick(2, "B", PickOutcome.Vetoed);
    b.Credit(p1, 1).Credit(p2, 2);
    b.Veto(p1, DrafterKind, 1).Veto(p1, CommunityKind, 1).Veto(p2, TeamKind, 1).Veto(p2, DrafterKind, 2);

    Values(Run(b, StatsMetrics.AvgVetoesPerDraft, StatsGroupBys.Series)).Should().Equal(new Dictionary<string, decimal> { ["Main"] = 2m });
  }

  // -------------------------------------------------------------------------
  // Filters
  // -------------------------------------------------------------------------

  private static QueryDataBuilder FilterData()
  {
    var b = new QueryDataBuilder()
      .Draft(1, "Main", "Standard", 1)
      .Draft(2, "Main", "Mega", 2)
      .Draft(3, "Bonus", "Standard", 3)
      .Draft(4, "Bonus", "Mega", null);

    foreach (var draft in new[] { 1, 2, 3, 4 })
    {
      b.Credit(b.Pick(draft, $"T{draft}"), 1);
    }

    return b;
  }

  [Fact]
  public void Run_ShouldFilterBySeries_CaseInsensitively()
  {
    var result = Run(FilterData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Draft, o => o.Series.Add("bOnUs"));

    Values(result).Keys.Should().BeEquivalentTo("Draft 3", "Draft 4");
  }

  [Fact]
  public void Run_ShouldFilterByDraftType_CaseInsensitively()
  {
    var result = Run(FilterData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Draft, o => o.DraftTypes.Add("mega"));

    Values(result).Keys.Should().BeEquivalentTo("Draft 2", "Draft 4");
  }

  [Fact]
  public void Run_ShouldApplyAnInclusiveEpisodeRange()
  {
    var result = Run(FilterData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Draft, o => { o.EpisodeFrom = 2; o.EpisodeTo = 3; });

    Values(result).Keys.Should().BeEquivalentTo("Draft 2", "Draft 3");
  }

  [Fact]
  public void Run_ShouldExcludeDraftsWithoutAnEpisodeNumber_WhenAnEpisodeFilterIsPresent()
  {
    var open = Run(FilterData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Draft, o => o.EpisodeFrom = 1);
    var none = Run(FilterData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Draft);

    Values(open).Keys.Should().BeEquivalentTo("Draft 1", "Draft 2", "Draft 3");
    Values(none).Keys.Should().Contain("Draft 4");
  }

  [Fact]
  public void Run_ShouldCombineFilters()
  {
    var result = Run(FilterData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Draft,
      o => { o.Series.Add("Main"); o.DraftTypes.Add("Mega"); });

    Values(result).Keys.Should().BeEquivalentTo("Draft 2");
  }

  [Fact]
  public void Run_ShouldKeepOnlyDrafters_WithEnoughAppearances_WhenMinAppearancesIsSet()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(2, "B"), 1).Credit(b.Pick(3, "C"), 1).Credit(b.Pick(1, "D"), 2);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter, o => o.MinAppearances = 2);

    Values(result).Keys.Should().BeEquivalentTo("Drafter 1");
    result.TotalGroups.Should().Be(1);
  }

  [Fact]
  public void Run_ShouldMeasureMinAppearancesOverTheFilteredDrafts()
  {
    var b = new QueryDataBuilder().Draft(1, "Main").Draft(2, "Main").Draft(3, "Bonus");
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(2, "B"), 1).Credit(b.Pick(3, "C"), 2).Credit(b.Pick(3, "D"), 1);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter, o => { o.MinAppearances = 2; o.Series.Add("Bonus"); });

    result.Rows.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Sorting, zero handling, ranks, limit
  // -------------------------------------------------------------------------

  private static QueryDataBuilder ZeroData()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(1, "B", PickOutcome.Vetoed), 2);
    return b;
  }

  [Fact]
  public void Run_ShouldDropZeroValuedGroups_WhenDescending()
  {
    var result = Run(ZeroData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    Values(result).Should().Equal(new Dictionary<string, decimal> { ["Drafter 1"] = 1 });
  }

  [Fact]
  public void Run_ShouldKeepZeroValuedGroups_WhenAscending()
  {
    var result = Run(ZeroData(), StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter, o => o.Ascending = true);

    result.Rows.Select(r => r.Name).Should().Equal("Drafter 2", "Drafter 1");
    result.Rows.Select(r => r.Value).Should().Equal(0m, 1m);
  }

  [Fact]
  public void Run_ShouldGiveTiedValuesTheSameRank_AndSkipTheNextRank_WhenDescending()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(1, "B"), 1);
    b.Credit(b.Pick(1, "C"), 2).Credit(b.Pick(1, "D"), 2);
    b.Credit(b.Pick(1, "E"), 3);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    result.Rows.Select(r => (r.Name, r.Rank)).Should().Equal(("Drafter 1", 1), ("Drafter 2", 1), ("Drafter 3", 3));
  }

  [Fact]
  public void Run_ShouldGiveTiedValuesTheSameRank_AndSkipTheNextRank_WhenAscending()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1);
    b.Credit(b.Pick(1, "B"), 2).Credit(b.Pick(1, "C"), 2);
    b.Credit(b.Pick(1, "D"), 3).Credit(b.Pick(1, "E"), 3);
    b.Credit(b.Pick(1, "F"), 4).Credit(b.Pick(1, "G"), 4).Credit(b.Pick(1, "H"), 4);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter, o => o.Ascending = true);

    result.Rows.Select(r => (r.Name, r.Rank)).Should().Equal(
      ("Drafter 1", 1), ("Drafter 2", 2), ("Drafter 3", 2), ("Drafter 4", 4));
  }

  [Fact]
  public void Run_ShouldHonourLimit_AndReportTheTotalNumberOfGroups()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(1, "B"), 1).Credit(b.Pick(1, "C"), 2).Credit(b.Pick(1, "D"), 3);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter, o => o.Limit = 1);

    result.Rows.Should().ContainSingle().Which.Name.Should().Be("Drafter 1");
    result.TotalGroups.Should().Be(3);
  }

  [Fact]
  public void Run_ShouldRankLimitedRowsAgainstTheWholeField()
  {
    var b = new QueryDataBuilder();
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(1, "B"), 2).Credit(b.Pick(1, "C"), 3).Credit(b.Pick(1, "D"), 3);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter, o => { o.Ascending = true; o.Limit = 1; });

    // Drafter 1 and 2 tie on 1; Drafter 3 has 2. Limit 1 shows only Drafter 1 at rank 1.
    result.Rows.Should().ContainSingle().Which.Rank.Should().Be(1);
  }

  [Fact]
  public void Run_ShouldBreakTiesByName_CaseInsensitively()
  {
    var b = new QueryDataBuilder().Draft(1, "beta").Draft(2, "Alpha").Draft(3, "gamma");
    b.Credit(b.Pick(1, "A"), 1).Credit(b.Pick(2, "B"), 1).Credit(b.Pick(3, "C"), 1);

    var result = Run(b, StatsMetrics.TitlesDrafted, StatsGroupBys.Series);

    result.Rows.Select(r => r.Name).Should().Equal("Alpha", "beta", "gamma");
  }

  [Fact]
  public void Run_ShouldReturnNoRows_WhenTheDatasetIsEmpty()
  {
    var result = Run(new QueryDataBuilder(), StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    result.Rows.Should().BeEmpty();
    result.TotalGroups.Should().Be(0);
  }

  [Fact]
  public void Run_ShouldThrow_WhenArgumentsAreNull()
  {
    var data = new QueryDataBuilder().Build();
    var spec = Spec(StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    FluentActions.Invoking(() => StatsQueryEngine.Run(null!, spec)).Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => StatsQueryEngine.Run(data, null!)).Should().Throw<ArgumentNullException>();
  }
}

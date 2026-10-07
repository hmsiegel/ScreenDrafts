using static ScreenDrafts.Modules.Reporting.IntegrationTests.Abstractions.StatsSeeder;

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Stats;

public sealed class QueryStatsTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly DateOnly _jan1 = new(2026, 1, 1);

  /// <summary>
  /// Alpha (Main Series, Standard, ep 1): Heat D1 | Alien D2 vetoed by D1 | Brazil D2 vetoed by D1 |
  ///   Casino D2 vetoed by D1 and overridden by D2 | Dune D1 vetoed by the community and overridden by D2 |
  ///   Elf D2 self-vetoed | Fargo D1 removed by the commissioner.
  /// Bravo (Main Series, Standard, ep 2): clean. Heat D1, Alien D2.
  /// Delta (Bonus Series, Mega, ep 3): clean. Heat D3.
  /// Charlie (Patreon Series, Speed, ep 4, policy 1): Ghost D1. Outside the canonical scope.
  /// </summary>
  private async Task SeedAsync()
  {
    var seed = new StatsSeeder(DbContext);

    var alpha = seed.Draft("Alpha").Part(episode: 1, mainFeed: _jan1);
    alpha.Pick("Heat", 1, 1);
    alpha.Pick("Alien", 2, 2).Veto(DrafterKind, 1);
    alpha.Pick("Brazil", 2, 2).Veto(DrafterKind, 1);
    alpha.Pick("Casino", 2, 2).Veto(DrafterKind, 1, overridden: true, overriddenBy: 2);
    alpha.Pick("Dune", 3, 1).Veto(CommunityKind, overridden: true, overriddenBy: 2);
    alpha.Pick("Elf", 4, 2).Veto(DrafterKind, 2);
    alpha.Pick("Fargo", 5, 1).Removed();

    var bravo = seed.Draft("Bravo").Part(episode: 2, mainFeed: _jan1.AddDays(7));
    bravo.Pick("Heat", 1, 1);
    bravo.Pick("Alien", 2, 2);

    seed.Draft("Delta", series: "Bonus Series", type: "Mega")
      .Part(episode: 3, mainFeed: _jan1.AddDays(14))
      .Pick("Heat", 1, 3);

    seed.Draft("Charlie", series: "Patreon Series", type: "Speed", policy: 1)
      .Part(episode: 4)
      .Pick("Ghost", 1, 1);

    await seed.SaveAsync(TestContext.Current.CancellationToken);
  }

  private async Task<QueryStatsResponse> QueryAsync(
    string metric,
    string groupBy,
    Func<QueryStatsQuery, QueryStatsQuery>? tweak = null)
  {
    var query = new QueryStatsQuery { Metric = metric, GroupBy = groupBy, IncludeAll = false };
    var result = await Sender.Send(tweak?.Invoke(query) ?? query, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    return result.Value;
  }

  private static Dictionary<string, decimal> Values(QueryStatsResponse response) =>
    response.Rows.ToDictionary(r => r.Name, r => r.Value);

  // -------------------------------------------------------------------------
  // One sample query per metric
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldCountLandedPicksPerDrafter_ForTitlesDraftedAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    response.Rows.Select(r => (r.Name, r.Value, r.Rank)).Should().Equal(
      ("Drafter 01", 3m, 1), // Heat, Dune (saved), Heat. Fargo was removed.
      ("Drafter 02", 2m, 2), // Casino (saved), Alien
      ("Drafter 03", 1m, 3));
    response.Rows[0].PublicId.Should().Be(PersonPublicId(1));
    response.Rows[0].Context.Should().Be("2 drafts");
    response.Metric.Should().Be("titlesDrafted");
    response.Format.Should().Be("count");
    response.IncludesNonCanonical.Should().BeFalse();
  }

  [Fact]
  public async Task Handle_ShouldCountPicksVetoedByTitle_ForPicksVetoedAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(StatsMetrics.PicksVetoed, StatsGroupBys.Title);

    Values(response).Should().Equal(new Dictionary<string, decimal> { ["Alien"] = 1, ["Brazil"] = 1, ["Elf"] = 1 });
  }

  [Fact]
  public async Task Handle_ShouldAttributeVetoesToDrafterIssuersOnly_ForVetoesUsedAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(StatsMetrics.VetoesUsed, StatsGroupBys.Drafter);

    Values(response).Should().Equal(new Dictionary<string, decimal> { ["Drafter 01"] = 3, ["Drafter 02"] = 1 });
  }

  [Fact]
  public async Task Handle_ShouldCountEveryVetoIncludingTheCommunitys_WhenGroupedByDraftAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(StatsMetrics.VetoesUsed, StatsGroupBys.Draft);

    Values(response).Should().Equal(new Dictionary<string, decimal> { ["Alpha"] = 5 });
  }

  [Fact]
  public async Task Handle_ShouldCountOverriddenVetoes_ForVetoesOverriddenAsync()
  {
    await SeedAsync();

    Values(await QueryAsync(StatsMetrics.VetoesOverridden, StatsGroupBys.Drafter))
      .Should().Equal(new Dictionary<string, decimal> { ["Drafter 01"] = 1 });
    Values(await QueryAsync(StatsMetrics.VetoesOverridden, StatsGroupBys.Series))
      .Should().Equal(new Dictionary<string, decimal> { ["Main Series"] = 2 });
  }

  [Fact]
  public async Task Handle_ShouldCountStandingSelfVetoes_ForSelfVetoesAsync()
  {
    await SeedAsync();

    Values(await QueryAsync(StatsMetrics.SelfVetoes, StatsGroupBys.Drafter))
      .Should().Equal(new Dictionary<string, decimal> { ["Drafter 02"] = 1 });
  }

  [Fact]
  public async Task Handle_ShouldCountRemovedPicks_ForCommissionerOverridesAsync()
  {
    await SeedAsync();

    Values(await QueryAsync(StatsMetrics.CommissionerOverrides, StatsGroupBys.Draft))
      .Should().Equal(new Dictionary<string, decimal> { ["Alpha"] = 1 });
    Values(await QueryAsync(StatsMetrics.CommissionerOverrides, StatsGroupBys.Drafter))
      .Should().Equal(new Dictionary<string, decimal> { ["Drafter 01"] = 1 });
  }

  [Fact]
  public async Task Handle_ShouldCountDistinctDraftsPerDrafter_ForAppearancesAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(StatsMetrics.Appearances, StatsGroupBys.Drafter);

    Values(response).Should().Equal(
      new Dictionary<string, decimal> { ["Drafter 01"] = 2, ["Drafter 02"] = 2, ["Drafter 03"] = 1 });
    response.Rows.Select(r => r.Rank).Should().Equal(1, 1, 3);
  }

  [Fact]
  public async Task Handle_ShouldCountCleanDraftsBySeries_ForCopaceticDraftsAsync()
  {
    await SeedAsync();

    Values(await QueryAsync(StatsMetrics.CopaceticDrafts, StatsGroupBys.Series))
      .Should().Equal(new Dictionary<string, decimal> { ["Main Series"] = 1, ["Bonus Series"] = 1 });
    Values(await QueryAsync(StatsMetrics.CopaceticDrafts, StatsGroupBys.Drafter))
      .Should().Equal(new Dictionary<string, decimal> { ["Drafter 01"] = 1, ["Drafter 02"] = 1, ["Drafter 03"] = 1 });
  }

  [Fact]
  public async Task Handle_ShouldDivideVetoesIssuedByAppearances_ForAvgVetoesPerDraftAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(StatsMetrics.AvgVetoesPerDraft, StatsGroupBys.Drafter);

    Values(response).Should().Equal(new Dictionary<string, decimal> { ["Drafter 01"] = 1.5m, ["Drafter 02"] = 0.5m });
    response.Format.Should().Be("ratio");
  }

  [Fact]
  public async Task Handle_ShouldDivideAllVetoesByDraftsPerSeries_ForAvgVetoesPerDraftAsync()
  {
    await SeedAsync();

    // Main Series: 5 vetoes over 2 drafts.
    Values(await QueryAsync(StatsMetrics.AvgVetoesPerDraft, StatsGroupBys.Series))
      .Should().Equal(new Dictionary<string, decimal> { ["Main Series"] = 2.5m });
  }

  // -------------------------------------------------------------------------
  // Filters, ordering, scope
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldFilterBySeriesCaseInsensitivelyAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(
      StatsMetrics.TitlesDrafted,
      StatsGroupBys.Drafter,
      q => q with { Series = ["main series"] });

    Values(response).Keys.Should().BeEquivalentTo("Drafter 01", "Drafter 02");
  }

  [Fact]
  public async Task Handle_ShouldFilterByDraftTypeAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(
      StatsMetrics.TitlesDrafted,
      StatsGroupBys.Drafter,
      q => q with { DraftTypes = ["MEGA"] });

    Values(response).Should().Equal(new Dictionary<string, decimal> { ["Drafter 03"] = 1 });
  }

  [Fact]
  public async Task Handle_ShouldApplyAnInclusiveEpisodeRangeAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(
      StatsMetrics.TitlesDrafted,
      StatsGroupBys.Draft,
      q => q with { EpisodeFrom = 2, EpisodeTo = 3 });

    Values(response).Should().Equal(new Dictionary<string, decimal> { ["Bravo"] = 2, ["Delta"] = 1 });
  }

  [Fact]
  public async Task Handle_ShouldApplyMinAppearancesAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(
      StatsMetrics.TitlesDrafted,
      StatsGroupBys.Drafter,
      q => q with { MinAppearances = 2 });

    Values(response).Keys.Should().BeEquivalentTo("Drafter 01", "Drafter 02");
  }

  [Fact]
  public async Task Handle_ShouldKeepZeroValuedGroups_OnlyWhenAscendingAsync()
  {
    await SeedAsync();

    var descending = await QueryAsync(StatsMetrics.VetoesUsed, StatsGroupBys.Drafter);
    var ascending = await QueryAsync(StatsMetrics.VetoesUsed, StatsGroupBys.Drafter, q => q with { Ascending = true });

    descending.Rows.Select(r => r.Name).Should().Equal("Drafter 01", "Drafter 02");
    ascending.Rows.Select(r => (r.Name, r.Value)).Should().Equal(
      ("Drafter 03", 0m), ("Drafter 02", 1m), ("Drafter 01", 3m));
  }

  [Fact]
  public async Task Handle_ShouldTruncateToTheLimit_AndReportTheTotalAsync()
  {
    await SeedAsync();

    var response = await QueryAsync(
      StatsMetrics.TitlesDrafted,
      StatsGroupBys.Drafter,
      q => q with { Limit = 1 });

    response.Rows.Should().ContainSingle().Which.Name.Should().Be("Drafter 01");
    response.TotalGroups.Should().Be(3);
    response.Truncated.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldExcludeNonCanonicalDrafts_WhenIncludeAllIsFalseAsync()
  {
    await SeedAsync();

    var canonical = await QueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Series);
    var all = await QueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Series, q => q with { IncludeAll = true });

    canonical.Rows.Select(r => r.Name).Should().NotContain("Patreon Series");
    all.Rows.Select(r => r.Name).Should().Contain("Patreon Series");
    all.IncludesNonCanonical.Should().BeTrue();
  }

  [Fact]
  public async Task Handle_ShouldCountATeamPickOnceAtDraftLevel_AndForEachMemberAsync()
  {
    var seed = new StatsSeeder(DbContext);
    seed.Draft("Teams").Part(episode: 1, mainFeed: _jan1).TeamPick("Heat", 1, team: 1, members: [1, 2]);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    Values(await QueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Draft))
      .Should().Equal(new Dictionary<string, decimal> { ["Teams"] = 1 });
    Values(await QueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter))
      .Should().Equal(new Dictionary<string, decimal> { ["Drafter 01"] = 1, ["Drafter 02"] = 1 });
  }

  // -------------------------------------------------------------------------
  // Validation
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnAProblem_WhenTheMetricAndGroupByDoNotPairAsync()
  {
    var result = await Sender.Send(
      new QueryStatsQuery { Metric = StatsMetrics.CopaceticDrafts, GroupBy = StatsGroupBys.Title, IncludeAll = false },
      TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Error!.Type.Should().Be(ErrorType.Problem);
    result.Error.Code.Should().Be("StatsQuery.IncompatibleGroupBy");
  }

  [Fact]
  public async Task Handle_ShouldReturnAProblem_WhenMinAppearancesIsUsedWithoutDrafterGroupingAsync()
  {
    var result = await Sender.Send(
      new QueryStatsQuery
      {
        Metric = StatsMetrics.TitlesDrafted,
        GroupBy = StatsGroupBys.Series,
        MinAppearances = 3,
        IncludeAll = false,
      },
      TestContext.Current.CancellationToken);

    result.Error!.Code.Should().Be("StatsQuery.MinAppearancesNeedsDrafterGrouping");
  }

  [Fact]
  public async Task Handle_ShouldReturnNoRows_WhenThereAreNoFactsAsync()
  {
    var response = await QueryAsync(StatsMetrics.TitlesDrafted, StatsGroupBys.Drafter);

    response.Rows.Should().BeEmpty();
    response.TotalGroups.Should().Be(0);
    response.Truncated.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Options
  // -------------------------------------------------------------------------

  private async Task<StatsQueryOptionsResponse> OptionsAsync(bool includeAll)
  {
    var result = await Sender.Send(
      new GetStatsQueryOptionsQuery { IncludeAll = includeAll },
      TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    return result.Value;
  }

  [Fact]
  public async Task Options_ShouldListOnlyCanonicalSeriesAndDraftTypes_WhenIncludeAllIsFalseAsync()
  {
    await SeedAsync();

    var options = await OptionsAsync(includeAll: false);

    options.CanIncludeAll.Should().BeFalse();
    options.Series.Should().Equal("Bonus Series", "Main Series");
    options.DraftTypes.Should().Equal("Mega", "Standard");
    options.MinEpisode.Should().Be(1);
    options.MaxEpisode.Should().Be(3);
  }

  [Fact]
  public async Task Options_ShouldListEverySeriesAndDraftType_WhenIncludeAllIsTrueAsync()
  {
    await SeedAsync();

    var options = await OptionsAsync(includeAll: true);

    options.CanIncludeAll.Should().BeTrue();
    options.Series.Should().Equal("Bonus Series", "Main Series", "Patreon Series");
    options.DraftTypes.Should().Equal("Mega", "Speed", "Standard");
    options.MaxEpisode.Should().Be(4);
  }

  [Fact]
  public async Task Options_ShouldListEveryMetricAndGroupByAsync()
  {
    var options = await OptionsAsync(includeAll: false);

    options.Metrics.Select(m => m.Code).Should().Equal(StatsMetrics.All.Select(m => m.Code));
    options.GroupBys.Select(g => g.Code).Should().Equal(StatsGroupBys.All.Select(g => g.Code));
    options.Series.Should().BeEmpty();
    options.MinEpisode.Should().BeNull();
  }

  [Fact]
  public async Task Options_ShouldNotListASeriesOnlyUsedByPolicyTwoDraftsWithoutAReleaseAsync()
  {
    var seed = new StatsSeeder(DbContext);
    seed.Draft("Unreleased", series: "Hidden Series", policy: 2).Part().Pick("Heat", 1, 1);
    await seed.SaveAsync(TestContext.Current.CancellationToken);

    (await OptionsAsync(includeAll: false)).Series.Should().NotContain("Hidden Series");
    (await OptionsAsync(includeAll: true)).Series.Should().Contain("Hidden Series");
  }
}

namespace ScreenDrafts.Modules.Reporting.UnitTests.QueryStats;

public sealed class StatsQueryValidatorTests
{
  private static QueryStatsQuery Query(
    string metric = StatsMetrics.TitlesDrafted,
    string groupBy = StatsGroupBys.Drafter,
    Action<Builder>? configure = null)
  {
    var b = new Builder();
    configure?.Invoke(b);

    return new QueryStatsQuery
    {
      Metric = metric,
      GroupBy = groupBy,
      Series = b.Series,
      DraftTypes = b.DraftTypes,
      EpisodeFrom = b.EpisodeFrom,
      EpisodeTo = b.EpisodeTo,
      MinAppearances = b.MinAppearances,
      Ascending = b.Ascending,
      Limit = b.Limit,
      IncludeAll = false,
    };
  }

  private sealed class Builder
  {
    public IReadOnlyList<string>? Series { get; set; }
    public IReadOnlyList<string>? DraftTypes { get; set; }
    public int? EpisodeFrom { get; set; }
    public int? EpisodeTo { get; set; }
    public int? MinAppearances { get; set; }
    public bool Ascending { get; set; }
    public int? Limit { get; set; }
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

  public static TheoryData<string, string> DisallowedPairs
  {
    get
    {
      var data = new TheoryData<string, string>();

      foreach (var metric in StatsMetrics.All)
      {
        foreach (var groupBy in StatsGroupBys.All.Where(g => !metric.GroupBys.Contains(g.Code)))
        {
          data.Add(metric.Code, groupBy.Code);
        }
      }

      return data;
    }
  }

  // -------------------------------------------------------------------------
  // Metric and group-by definitions
  // -------------------------------------------------------------------------

  [Fact]
  public void Metrics_ShouldOnlyListKnownGroupBys()
  {
    foreach (var metric in StatsMetrics.All)
    {
      metric.GroupBys.Should().NotBeEmpty();
      metric.GroupBys.Should().OnlyContain(g => StatsGroupBys.IsKnown(g), $"metric '{metric.Code}' lists only known group-bys");
    }
  }

  [Theory]
  [InlineData(StatsMetrics.CopaceticDrafts, StatsGroupBys.Title)]
  [InlineData(StatsMetrics.CopaceticDrafts, StatsGroupBys.Draft)]
  [InlineData(StatsMetrics.Appearances, StatsGroupBys.Title)]
  [InlineData(StatsMetrics.Appearances, StatsGroupBys.Draft)]
  public void Metrics_ShouldNotAllowTitleOrDraftGrouping_ForCopaceticDraftsAndAppearances(string metric, string groupBy)
  {
    StatsMetrics.Find(metric)!.GroupBys.Should().NotContain(groupBy);
  }

  [Fact]
  public void Metrics_ShouldHaveUniqueCodes()
  {
    StatsMetrics.All.Select(m => m.Code).Should().OnlyHaveUniqueItems();
    StatsGroupBys.All.Select(g => g.Code).Should().OnlyHaveUniqueItems();
  }

  // -------------------------------------------------------------------------
  // Metric / group-by pairing
  // -------------------------------------------------------------------------

  [Theory]
  [MemberData(nameof(AllowedPairs))]
  public void Validate_ShouldSucceed_WhenPairingIsAllowed(string metric, string groupBy)
  {
    var result = StatsQueryValidator.Validate(Query(metric, groupBy));

    result.Error.Should().BeNull();
    result.Spec.Should().NotBeNull();
    result.Spec.Metric.Should().Be(metric);
    result.Spec.GroupBy.Should().Be(groupBy);
  }

  [Theory]
  [MemberData(nameof(DisallowedPairs))]
  public void Validate_ShouldReturnProblem_WhenPairingIsNotAllowed(string metric, string groupBy)
  {
    var result = StatsQueryValidator.Validate(Query(metric, groupBy));

    result.Spec.Should().BeNull();
    result.Error!.Code.Should().Be("StatsQuery.IncompatibleGroupBy");
    result.Error.Type.Should().Be(ErrorType.Problem);
  }

  [Fact]
  public void Validate_ShouldReturnProblem_WhenMetricIsUnknown()
  {
    var result = StatsQueryValidator.Validate(Query("nonsense"));

    result.Error!.Code.Should().Be("StatsQuery.UnknownMetric");
    result.Error.Type.Should().Be(ErrorType.Problem);
  }

  [Fact]
  public void Validate_ShouldReturnProblem_WhenGroupByIsUnknown()
  {
    var result = StatsQueryValidator.Validate(Query(groupBy: "nonsense"));

    result.Error!.Code.Should().Be("StatsQuery.UnknownGroupBy");
  }

  // -------------------------------------------------------------------------
  // Limit
  // -------------------------------------------------------------------------

  [Fact]
  public void Validate_ShouldDefaultLimitToTwentyFive_WhenNoneSupplied()
  {
    StatsQueryValidator.Validate(Query()).Spec!.Limit.Should().Be(25);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(100)]
  public void Validate_ShouldAcceptLimitBoundaries(int limit)
  {
    StatsQueryValidator.Validate(Query(configure: b => b.Limit = limit)).Spec!.Limit.Should().Be(limit);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  [InlineData(101)]
  public void Validate_ShouldReturnProblem_WhenLimitIsOutOfRange(int limit)
  {
    StatsQueryValidator.Validate(Query(configure: b => b.Limit = limit)).Error!.Code
      .Should().Be("StatsQuery.InvalidLimit");
  }

  // -------------------------------------------------------------------------
  // minAppearances
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(1)]
  [InlineData(500)]
  public void Validate_ShouldAcceptMinAppearancesBoundaries_WhenGroupedByDrafter(int min)
  {
    var result = StatsQueryValidator.Validate(Query(configure: b => b.MinAppearances = min));

    result.Spec!.MinAppearances.Should().Be(min);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(501)]
  public void Validate_ShouldReturnProblem_WhenMinAppearancesIsOutOfRange(int min)
  {
    StatsQueryValidator.Validate(Query(configure: b => b.MinAppearances = min)).Error!.Code
      .Should().Be("StatsQuery.InvalidMinAppearances");
  }

  [Fact]
  public void Validate_ShouldReturnProblem_WhenMinAppearancesIsUsedWithoutDrafterGrouping()
  {
    var result = StatsQueryValidator.Validate(Query(groupBy: StatsGroupBys.Series, configure: b => b.MinAppearances = 5));

    result.Error!.Code.Should().Be("StatsQuery.MinAppearancesNeedsDrafterGrouping");
  }

  // -------------------------------------------------------------------------
  // Episode range and filters
  // -------------------------------------------------------------------------

  [Fact]
  public void Validate_ShouldReturnProblem_WhenEpisodeFromExceedsEpisodeTo()
  {
    var result = StatsQueryValidator.Validate(Query(configure: b => { b.EpisodeFrom = 10; b.EpisodeTo = 9; }));

    result.Error!.Code.Should().Be("StatsQuery.InvalidEpisodeRange");
  }

  [Fact]
  public void Validate_ShouldAcceptEqualEpisodeFromAndTo_AndOpenEndedRanges()
  {
    StatsQueryValidator.Validate(Query(configure: b => { b.EpisodeFrom = 7; b.EpisodeTo = 7; })).Spec.Should().NotBeNull();
    StatsQueryValidator.Validate(Query(configure: b => b.EpisodeFrom = 7)).Spec.Should().NotBeNull();
    StatsQueryValidator.Validate(Query(configure: b => b.EpisodeTo = 7)).Spec.Should().NotBeNull();
  }

  [Fact]
  public void Validate_ShouldAcceptFiftyFilterValues_AndRejectFiftyOne()
  {
    var fifty = Enumerable.Range(0, 50).Select(i => $"s{i}").ToList();
    var fiftyOne = Enumerable.Range(0, 51).Select(i => $"s{i}").ToList();

    StatsQueryValidator.Validate(Query(configure: b => b.Series = fifty)).Spec.Should().NotBeNull();
    StatsQueryValidator.Validate(Query(configure: b => b.Series = fiftyOne)).Error!.Code
      .Should().Be("StatsQuery.TooManyFilterValues");
    StatsQueryValidator.Validate(Query(configure: b => b.DraftTypes = fiftyOne)).Error!.Code
      .Should().Be("StatsQuery.TooManyFilterValues");
  }

  [Fact]
  public void Validate_ShouldBuildCaseInsensitiveFilterSets()
  {
    var spec = StatsQueryValidator.Validate(Query(configure: b => { b.Series = ["Main"]; b.DraftTypes = ["Standard"]; })).Spec!;

    spec.Series.Contains("main").Should().BeTrue();
    spec.DraftTypes.Contains("STANDARD").Should().BeTrue();
  }

  [Fact]
  public void Validate_ShouldDefaultFiltersToEmptyAndAscendingToFalse()
  {
    var spec = StatsQueryValidator.Validate(Query()).Spec!;

    spec.Series.Should().BeEmpty();
    spec.DraftTypes.Should().BeEmpty();
    spec.EpisodeFrom.Should().BeNull();
    spec.EpisodeTo.Should().BeNull();
    spec.MinAppearances.Should().BeNull();
    spec.Ascending.Should().BeFalse();
  }

  [Fact]
  public void Validate_ShouldCarryAscendingFlag()
  {
    StatsQueryValidator.Validate(Query(configure: b => b.Ascending = true)).Spec!.Ascending.Should().BeTrue();
  }

  [Fact]
  public void Validate_ShouldThrow_WhenQueryIsNull()
  {
    FluentActions.Invoking(() => StatsQueryValidator.Validate(null!)).Should().Throw<ArgumentNullException>();
  }
}

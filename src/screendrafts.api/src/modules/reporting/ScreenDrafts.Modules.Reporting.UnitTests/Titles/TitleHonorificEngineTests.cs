using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.TitleAppearanceBuilder;

namespace ScreenDrafts.Modules.Reporting.UnitTests.Titles;

public sealed class TitleHonorificEngineTests
{
  private static TitleHonorificResult Run(IEnumerable<TitleAppearanceRow> rows, TitleHonorificSpec? spec = null) =>
    TitleHonorificEngine.Run([.. rows], spec ?? Spec());

  private static IEnumerable<TitleAppearanceRow> Times(string title, int count, int firstEpisode = 1) =>
    Enumerable
      .Range(0, count)
      .Select(i => Appearance(title, $"D{firstEpisode + i}", firstEpisode + i, $"2026-01-{firstEpisode + i:00}", firstEpisode + i));

  // -------------------------------------------------------------------------
  // Levels and join numbering
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(TitleHonorificLevels.MarqueeOfFame, 2)]
  [InlineData(TitleHonorificLevels.HatTrick, 3)]
  [InlineData(TitleHonorificLevels.GrandSlam, 4)]
  [InlineData(TitleHonorificLevels.HighFive, 5)]
  public void Run_ShouldJoinATitleOnTheAppearanceThatReachesTheLevelMinimum(string level, int minimum)
  {
    var rows = Times("Heat", minimum + 1);

    var entry = Run(rows, Spec(level)).Titles.Single();

    entry.AppearanceCount.Should().Be(minimum + 1);
    entry.JoinedEpisode.Should().Be(minimum);
    entry.FirstEpisode.Should().Be(1);
  }

  [Fact]
  public void Run_ShouldNotListATitleBelowTheLevelMinimum()
  {
    var result = Run(Times("Heat", 2), Spec(TitleHonorificLevels.HatTrick));

    result.Titles.Should().BeEmpty();
    result.TotalMatching.Should().Be(0);
  }

  [Fact]
  public void Run_ShouldNumberTitlesInTheOrderTheyJoin()
  {
    // Zulu reaches 2 on episode 4, Alpha on episode 3, Mike on episode 6.
    var rows = new[]
    {
      Appearance("Zulu", "D1", 1, "2026-01-01", 1),
      Appearance("Alpha", "D2", 2, "2026-01-02", 2),
      Appearance("Alpha", "D3", 3, "2026-01-03", 3),
      Appearance("Zulu", "D4", 4, "2026-01-04", 4),
      Appearance("Mike", "D5", 5, "2026-01-05", 5),
      Appearance("Mike", "D6", 6, "2026-01-06", 6),
    };

    var titles = Run(rows, Spec(sort: TitleHonorificSorts.Oldest)).Titles;

    titles.Select(t => (t.Title, t.Number)).Should().Equal(("Alpha", 1), ("Zulu", 2), ("Mike", 3));
  }

  [Fact]
  public void Run_ShouldNumberTitlesPerLevelIndependently()
  {
    var rows = Times("Heat", 3).Concat(Times("Alien", 2, firstEpisode: 4));

    var marquee = Run(rows, Spec(TitleHonorificLevels.MarqueeOfFame, sort: TitleHonorificSorts.Oldest)).Titles;
    var hatTrick = Run(rows, Spec(TitleHonorificLevels.HatTrick, sort: TitleHonorificSorts.Oldest)).Titles;

    marquee.Select(t => (t.Title, t.Number)).Should().Equal(("Heat", 1), ("Alien", 2));
    hatTrick.Select(t => (t.Title, t.Number)).Should().Equal(("Heat", 1));
  }

  // -------------------------------------------------------------------------
  // Ordering of appearances
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldOrderAppearancesByPartReleaseDate_WhenEpisodeNumbersDisagree()
  {
    // Draft "Epic" is episode 10 for both parts but its parts released weeks apart,
    // and draft "Between" (episode 11) came out between them.
    var rows = new[]
    {
      Appearance("Heat", "Epic", 10, "2026-01-01", 1, part: 1, totalParts: 2),
      Appearance("Heat", "Between", 11, "2026-02-01", 2),
      Appearance("Heat", "Epic", 10, "2026-03-01", 3, part: 2, totalParts: 2),
    };

    var entry = Run(rows, Spec(TitleHonorificLevels.HatTrick)).Titles.Single();

    entry.Appearances.Select(a => (a.DraftTitle, a.PartIndex)).Should().Equal(("Epic", 1), ("Between", 1), ("Epic", 2));
    entry.JoinedDraftTitle.Should().Be("Epic");
    entry.JoinedPartIndex.Should().Be(2);
    entry.JoinedOn.Should().Be("2026-03-01");
  }

  [Fact]
  public void Run_ShouldUseThePartsOwnReleaseDate_NotTheDraftsFirstPart()
  {
    var rows = new[]
    {
      Appearance("Heat", "Early", 1, "2026-01-01", 1),
      Appearance("Heat", "Epic", 5, "2026-04-15", 4, part: 4, totalParts: 4),
    };

    var entry = Run(rows).Titles.Single();

    entry.JoinedOn.Should().Be("2026-04-15");
    entry.Appearances[1].ReleasedOn.Should().Be("2026-04-15");
  }

  [Fact]
  public void Run_ShouldBreakReleaseDateTies_ByEpisodeThenPartThenSubDraftThenPlayOrder()
  {
    var rows = new[]
    {
      Appearance("Heat", "X", 2, "2026-01-01", 1, play: 9, part: 2),
      Appearance("Heat", "X", 2, "2026-01-01", 1, play: 1, part: 1, subDraft: 1),
      Appearance("Heat", "X", 2, "2026-01-01", 1, play: 5, part: 1, subDraft: 0),
      Appearance("Heat", "X", 1, "2026-01-01", 1, play: 7, part: 3),
    };

    var entry = Run(rows, Spec(TitleHonorificLevels.HighFive, sort: TitleHonorificSorts.Oldest)).Titles;

    entry.Should().BeEmpty(); // only four appearances
    var appearances = Run(rows, Spec(TitleHonorificLevels.GrandSlam)).Titles.Single().Appearances;
    appearances.Select(a => (a.EpisodeNumber, a.PartIndex, a.Position)).Should().Equal(
      (1, 3, 7),
      (2, 1, 5),
      (2, 1, 1),
      (2, 2, 9));
  }

  [Fact]
  public void Run_ShouldNumberTitlesJoiningOnTheSamePartByPlayOrder()
  {
    // Both titles reach their second appearance in the same part. Their names sort the other way round.
    var rows = new[]
    {
      Appearance("Alpha", "D1", 1, "2026-01-01", 1, play: 1),
      Appearance("Zulu", "D1", 1, "2026-01-01", 1, play: 2),
      Appearance("Zulu", "D2", 2, "2026-01-08", 2, play: 3),
      Appearance("Alpha", "D2", 2, "2026-01-08", 2, play: 8),
    };

    var titles = Run(rows, Spec(sort: TitleHonorificSorts.Oldest)).Titles;

    titles.Select(t => (t.Title, t.Number)).Should().Equal(("Zulu", 1), ("Alpha", 2));
  }

  [Fact]
  public void Run_ShouldSortAPartWithNoReleaseDateLast()
  {
    var rows = new[]
    {
      Appearance("Heat", "Unreleased", 1, null, null),
      Appearance("Heat", "Released", 9, "2026-05-01", 7),
      Appearance("Heat", "AlsoReleased", 8, "2026-04-01", 6),
    };

    var entry = Run(rows, Spec(TitleHonorificLevels.HatTrick)).Titles.Single();

    entry.Appearances.Select(a => a.DraftTitle).Should().Equal("AlsoReleased", "Released", "Unreleased");
    entry.GapEpisodes.Should().BeNull();
    entry.GapDays.Should().BeNull();
    entry.JoinedOn.Should().BeNull();
  }

  [Fact]
  public void Run_ShouldNumberTitlesJoiningOnAnUnreleasedPartAfterReleasedOnes()
  {
    var rows = new[]
    {
      Appearance("Late", "D1", 1, "2026-01-01", 1),
      Appearance("Late", "Pending", 2, null, null),
      Appearance("Early", "D2", 2, "2026-01-02", 2),
      Appearance("Early", "D3", 3, "2026-01-03", 3),
    };

    var titles = Run(rows, Spec(sort: TitleHonorificSorts.Oldest)).Titles;

    titles.Select(t => t.Title).Should().Equal("Early", "Late");
  }

  // -------------------------------------------------------------------------
  // Gaps
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldMeasureGapEpisodesByReleaseRank_AndGapDaysByDate()
  {
    var rows = new[]
    {
      Appearance("Heat", "D1", 1, "2026-01-01", 5),
      Appearance("Heat", "D2", 2, "2026-01-31", 9),
    };

    var entry = Run(rows).Titles.Single();

    entry.GapEpisodes.Should().Be(4);
    entry.GapDays.Should().Be(30);
  }

  [Fact]
  public void Run_ShouldReportAGapOfOne_WhenTheVeryNextReleaseJoinsTheLevel()
  {
    var rows = new[]
    {
      Appearance("Heat", "D1", 1, "2026-01-01", 5),
      Appearance("Heat", "D2", 2, "2026-01-02", 6),
    };

    var entry = Run(rows).Titles.Single();

    entry.GapEpisodes.Should().Be(1);
    entry.GapDays.Should().Be(1);
  }

  [Fact]
  public void Run_ShouldCountReleasedPartsInTheGap_WhenATitleJoinsOnPartFour()
  {
    // First appearance is release 10. It joins on Part 4 of a multi-part draft released as release 14.
    var rows = new[]
    {
      Appearance("Heat", "Solo", 3, "2026-02-01", 10),
      Appearance("Heat", "Epic", 4, "2026-03-15", 14, part: 4, totalParts: 4),
    };

    var entry = Run(rows).Titles.Single();

    entry.JoinedPartIndex.Should().Be(4);
    entry.JoinedTotalParts.Should().Be(4);
    entry.JoinedOn.Should().Be("2026-03-15");
    entry.GapEpisodes.Should().Be(4);
    entry.GapDays.Should().Be(42);
  }

  [Fact]
  public void Run_ShouldUseTheLevelJoinAppearance_NotTheLatest_ForTheGap()
  {
    var rows = new[]
    {
      Appearance("Heat", "D1", 1, "2026-01-01", 1),
      Appearance("Heat", "D2", 2, "2026-01-02", 2),
      Appearance("Heat", "D3", 3, "2026-06-01", 50),
    };

    var entry = Run(rows, Spec(TitleHonorificLevels.MarqueeOfFame)).Titles.Single();

    entry.AppearanceCount.Should().Be(3);
    entry.GapEpisodes.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Counts, search, sort, paging
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldListTheSizeOfEveryLevel()
  {
    var rows = Times("Two", 2)
      .Concat(Times("Three", 3, 10))
      .Concat(Times("Four", 4, 20))
      .Concat(Times("Five", 5, 40))
      .Concat(Times("Six", 6, 50))
      .Concat(Times("One", 1, 70));

    var counts = Run(rows).Counts;

    counts.Select(c => (c.Code, c.Count)).Should().Equal(
      ("marquee-of-fame", 5),
      ("hat-trick", 4),
      ("grand-slam", 3),
      ("high-five", 2));
    counts.Select(c => c.Label).Should().Equal("Marquee of Fame", "Hat Trick", "Grand Slam", "High Five");
  }

  [Fact]
  public void Run_ShouldKeepTheCounts_RegardlessOfSearchLevelOrPaging()
  {
    var rows = Times("Two", 2).Concat(Times("Three", 3, 10));

    var narrowed = Run(rows, Spec(TitleHonorificLevels.HatTrick, search: "nothing", pageSize: 1));

    narrowed.Titles.Should().BeEmpty();
    narrowed.Counts.Select(c => c.Count).Should().Equal(2, 1, 0, 0);
  }

  [Fact]
  public void Run_ShouldKeepJoinNumbers_WhenSearchingAndSorting()
  {
    var rows = new[]
    {
      Appearance("Alpha", "D1", 1, "2026-01-01", 1),
      Appearance("Alpha", "D2", 2, "2026-01-02", 2),
      Appearance("Bravo", "D3", 3, "2026-01-03", 3),
      Appearance("Bravo", "D4", 4, "2026-01-04", 4),
      Appearance("Charlie", "D5", 5, "2026-01-05", 5),
      Appearance("Charlie", "D6", 6, "2026-01-06", 6),
    };

    var searched = Run(rows, Spec(search: "char")).Titles;
    var alphabetical = Run(rows, Spec(sort: TitleHonorificSorts.Alphabetical)).Titles;

    searched.Should().ContainSingle().Which.Number.Should().Be(3);
    alphabetical.Select(t => (t.Title, t.Number)).Should().Equal(("Alpha", 1), ("Bravo", 2), ("Charlie", 3));
  }

  [Fact]
  public void Run_ShouldSearchCaseInsensitivelyBySubstring_AndTrimTheTerm()
  {
    var rows = Times("The Heat", 2).Concat(Times("Alien", 2, 10));

    var result = Run(rows, Spec(search: "  HEA  "));

    result.Titles.Select(t => t.Title).Should().Equal("The Heat");
    result.TotalMatching.Should().Be(1);
  }

  [Fact]
  public void Run_ShouldIgnoreABlankSearch()
  {
    var rows = Times("Heat", 2).Concat(Times("Alien", 2, 10));

    Run(rows, Spec(search: "   ")).TotalMatching.Should().Be(2);
  }

  private static IEnumerable<TitleAppearanceRow> ThreeTitles() =>
    Times("Charlie", 2)
      .Concat(Times("Alpha", 4, 10))
      .Concat(Times("Bravo", 3, 30));

  [Fact]
  public void Run_ShouldSortNewestFirstByDefault()
  {
    Run(ThreeTitles()).Titles.Select(t => t.Title).Should().Equal("Bravo", "Alpha", "Charlie");
  }

  [Fact]
  public void Run_ShouldSortOldestFirst()
  {
    Run(ThreeTitles(), Spec(sort: TitleHonorificSorts.Oldest)).Titles.Select(t => t.Title).Should().Equal("Charlie", "Alpha", "Bravo");
  }

  [Fact]
  public void Run_ShouldSortAlphabetically()
  {
    Run(ThreeTitles(), Spec(sort: TitleHonorificSorts.Alphabetical)).Titles.Select(t => t.Title).Should().Equal("Alpha", "Bravo", "Charlie");
  }

  [Fact]
  public void Run_ShouldSortByAppearanceCountThenJoinOrder()
  {
    Run(ThreeTitles(), Spec(sort: TitleHonorificSorts.Appearances)).Titles.Select(t => t.Title).Should().Equal("Alpha", "Bravo", "Charlie");
  }

  [Fact]
  public void Run_ShouldPageResults_AndReportTotalMatching()
  {
    var rows = Enumerable
      .Range(1, 5)
      .SelectMany(i => new[]
      {
        Appearance($"T{i}", $"A{i}", i * 2, $"2026-01-{i * 2:00}", i * 2),
        Appearance($"T{i}", $"B{i}", i * 2 + 1, $"2026-01-{i * 2 + 1:00}", i * 2 + 1),
      });

    var second = Run(rows, Spec(sort: TitleHonorificSorts.Oldest, page: 2, pageSize: 2));
    var last = Run(rows, Spec(sort: TitleHonorificSorts.Oldest, page: 3, pageSize: 2));
    var beyond = Run(rows, Spec(sort: TitleHonorificSorts.Oldest, page: 4, pageSize: 2));

    second.Titles.Select(t => t.Number).Should().Equal(3, 4);
    last.Titles.Select(t => t.Number).Should().Equal(5);
    beyond.Titles.Should().BeEmpty();
    beyond.TotalMatching.Should().Be(5);
  }

  [Fact]
  public void Run_ShouldNumberAppearancesFromOne_AndCarryThePositionAndParts()
  {
    var rows = new[]
    {
      Appearance("Heat", "D1", 1, "2026-01-01", 1, play: 4, part: 2, totalParts: 3),
      Appearance("Heat", "D2", 2, "2026-01-02", 2, play: 7),
    };

    var entry = Run(rows).Titles.Single();

    entry.Appearances.Select(a => a.AppearanceNumber).Should().Equal(1, 2);
    entry.Appearances[0].PartIndex.Should().Be(2);
    entry.Appearances[0].TotalParts.Should().Be(3);
    entry.Appearances[0].Position.Should().Be(4);
    entry.FirstDraftTitle.Should().Be("D1");
    entry.FirstOn.Should().Be("2026-01-01");
  }

  [Fact]
  public void Run_ShouldReturnNothing_WhenThereAreNoRows()
  {
    var result = Run([]);

    result.Titles.Should().BeEmpty();
    result.TotalMatching.Should().Be(0);
    result.Counts.Should().HaveCount(4).And.OnlyContain(c => c.Count == 0);
  }

  [Fact]
  public void Run_ShouldThrow_WhenArgumentsAreNull()
  {
    FluentActions.Invoking(() => TitleHonorificEngine.Run(null!, Spec())).Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => TitleHonorificEngine.Run([], null!)).Should().Throw<ArgumentNullException>();
  }
}

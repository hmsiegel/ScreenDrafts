using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.TitleAppearanceBuilder;

namespace ScreenDrafts.Modules.Reporting.UnitTests.Titles;

public sealed class TitleHonorificPositionTests
{
  private static TitleHonorificResult Run(
    IEnumerable<TitleAppearanceRow> rows,
    string level,
    string? search = null
  ) => TitleHonorificEngine.Run([.. rows], Spec(level, search));

  /// <summary>One appearance per position, each in its own episode and release, in order.</summary>
  private static IEnumerable<TitleAppearanceRow> Placed(string title, params int[] positions) =>
    positions.Select(
      (position, i) =>
        Appearance(title, $"D{i + 1}", i + 1, $"2026-01-{i + 1:00}", i + 1, position: position)
    );

  // -------------------------------------------------------------------------
  // Unified No. 1
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldJoinUnifiedNo1_OnTheSecondNumberOneFinish()
  {
    var rows = Placed("Heat", 1, 3, 1).Concat(Placed("Alien", 1, 2, 3));

    var result = Run(rows, TitleHonorificLevels.UnifiedNo1);

    var heat = result.Titles.Should().ContainSingle().Subject;
    heat.Title.Should().Be("Heat");
    heat.JoinedEpisode.Should().Be(3);
    heat.AppearanceCount.Should().Be(3);
    result.TotalMatching.Should().Be(1);
  }

  [Fact]
  public void Run_ShouldNotListATitleInUnifiedNo1_WithASingleNumberOneFinish()
  {
    var result = Run(Placed("Heat", 1, 2, 3, 4), TitleHonorificLevels.UnifiedNo1);

    result.Titles.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // The Cycle
  // -------------------------------------------------------------------------

  [Fact]
  public void Run_ShouldJoinTheCycle_OnTheAppearanceThatCompletesIt()
  {
    var result = Run(Placed("Heat", 1, 1, 2, 3, 4, 7), TitleHonorificLevels.TheCycle);

    var heat = result.Titles.Should().ContainSingle().Subject;
    heat.JoinedEpisode.Should().Be(5);
    heat.AppearanceCount.Should().Be(6);
    result.NearMisses.Should().BeEmpty();
  }

  [Fact]
  public void Run_ShouldNotListATitleInTheCycle_ThatLacksOnePlace()
  {
    var result = Run(Placed("Heat", 1, 2, 4), TitleHonorificLevels.TheCycle);

    result.Titles.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Close to The Cycle
  // -------------------------------------------------------------------------

  private static IEnumerable<TitleAppearanceRow> NearMissRows() =>
    Placed("Heat", 1, 2, 4)
      .Concat(Placed("Alien", 1, 1, 2, 3, 5))
      .Concat(Placed("Done", 1, 2, 3, 4))
      .Concat(Placed("Two", 1, 2));

  [Fact]
  public void Run_ShouldListTitlesOnePlaceShortOfTheCycle_WithTheMissingPlace()
  {
    var result = Run(NearMissRows(), TitleHonorificLevels.TheCycle);

    // Done has all four places and Two has only two, so neither is a near miss.
    result.Titles.Select(t => t.Title).Should().Equal("Done");
    result.NearMisses.Select(n => (n.Title, n.MissingPosition)).Should().Equal(("Heat", 3), ("Alien", 4));
    result.NearMisses[0].PositionsHeld.Should().Equal(1, 2, 4);
    result.NearMisses[1].PositionsHeld.Should().Equal(1, 2, 3);
    result.NearMisses[1].AppearanceCount.Should().Be(5);
  }

  [Fact]
  public void Run_ShouldApplyTheSearchToNearMisses()
  {
    var result = Run(NearMissRows(), TitleHonorificLevels.TheCycle, search: "ali");

    result.NearMisses.Select(n => n.Title).Should().Equal("Alien");
  }

  [Theory]
  [InlineData(TitleHonorificLevels.MarqueeOfFame)]
  [InlineData(TitleHonorificLevels.UnifiedNo1)]
  public void Run_ShouldLeaveNearMissesEmpty_ForEveryLevelButTheCycle(string level)
  {
    Run(NearMissRows(), level).NearMisses.Should().BeEmpty();
  }

  [Fact]
  public void Run_ShouldCountTitlesAtTheTwoPositionLevels()
  {
    var counts = Run(NearMissRows(), TitleHonorificLevels.MarqueeOfFame).Counts;

    // Alien finished at No. 1 twice. Done is the only title to complete The Cycle.
    counts.Single(c => c.Code == TitleHonorificLevels.UnifiedNo1).Count.Should().Be(1);
    counts.Single(c => c.Code == TitleHonorificLevels.TheCycle).Count.Should().Be(1);
  }
}

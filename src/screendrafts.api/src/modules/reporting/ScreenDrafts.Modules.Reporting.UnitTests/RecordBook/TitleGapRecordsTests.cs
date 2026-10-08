using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.RecordBookTestData;
using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.TitleAppearanceBuilder;

namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class TitleGapRecordsTests
{
  // Heat: ranks 1, 5, 6, 20. Alien: ranks 2, 3, 30. Jaws: ranks 4, 7.
  private static readonly TitleAppearanceRow[] _rows =
  [
    Appearance("Heat", "A", 1, "2020-01-01", 1),
    Appearance("Heat", "B", 5, "2020-02-01", 5),
    Appearance("Heat", "C", 6, "2020-02-08", 6),
    Appearance("Heat", "D", 20, "2021-01-01", 20),
    Appearance("Alien", "E", 2, "2020-01-08", 2),
    Appearance("Alien", "F", 3, "2020-01-15", 3),
    Appearance("Alien", "G", 30, "2022-01-01", 30),
    Appearance("Jaws", "H", 4, "2020-01-22", 4),
    Appearance("Jaws", "I", 7, "2020-02-15", 7),
  ];

  [Theory]
  [InlineData("join-mof", 1, "Alien", 4, "Heat")]
  [InlineData("mof-hat-trick", 1, "Heat", 27, "Alien")]
  [InlineData("first-hat-trick", 5, "Heat", 28, "Alien")]
  [InlineData("mof-grand-slam", 15, "Heat", 15, "Heat")]
  [InlineData("first-grand-slam", 19, "Heat", 19, "Heat")]
  [InlineData("hat-trick-grand-slam", 14, "Heat", 14, "Heat")]
  public void Build_ShouldRankShortestAndLongestGap(
    string gap, int shortest, string shortestTitle, int longest, string longestTitle)
  {
    var section = TitleRecordsCalculator.Build([], _rows);

    var low = Find(section, $"title.gap.{gap}.shortest");
    var high = Find(section, $"title.gap.{gap}.longest");

    low!.Value.Should().Be(shortest);
    low.Holders.Single().Name.Should().Be(shortestTitle);
    high!.Value.Should().Be(longest);
    high.Holders.Single().Name.Should().Be(longestTitle);
  }

  [Fact]
  public void Build_ShouldOmitGapRecords_WhenNoTitleReachedTheLaterAppearance()
  {
    var section = TitleRecordsCalculator.Build([], _rows.Where(r => r.MediaTitle == "Jaws").ToList());

    Find(section, "title.gap.join-mof.shortest").Should().NotBeNull();
    Find(section, "title.gap.mof-hat-trick.shortest").Should().BeNull();
    Find(section, "title.gap.hat-trick-grand-slam.longest").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldSkipATitle_WhenEitherAppearanceHasNoMainFeedRelease()
  {
    var rows = new[]
    {
      Appearance("Heat", "A", 1, "2020-01-01", 1),
      Appearance("Heat", "B", 5, "2020-02-01", 5),
      Appearance("Heat", "C", null, null, null),
    };

    var section = TitleRecordsCalculator.Build([], rows);

    Find(section, "title.gap.join-mof.shortest")!.Value.Should().Be(4);
    Find(section, "title.gap.mof-hat-trick.shortest").Should().BeNull();
    Find(section, "title.gap.first-hat-trick.shortest").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldReturnEveryTiedTitleSortedByName()
  {
    var rows = new[]
    {
      Appearance("Zulu", "A", 1, "2020-01-01", 1),
      Appearance("Zulu", "B", 3, "2020-01-15", 3),
      Appearance("Alpha", "C", 10, "2020-03-01", 10),
      Appearance("Alpha", "D", 12, "2020-03-15", 12),
    };

    var record = Find(TitleRecordsCalculator.Build([], rows), "title.gap.join-mof.shortest");

    record!.Value.Should().Be(2);
    record.Holders.Select(h => h.Name).Should().Equal("Alpha", "Zulu");
  }

  [Fact]
  public void Build_ShouldDescribeTheGapOnTheHolder()
  {
    var record = Find(TitleRecordsCalculator.Build([], _rows), "title.gap.join-mof.longest");

    record!.Holders.Single().Context.Should().Be("Ep. 1 to Ep. 5 · 31 days");
  }

  [Fact]
  public void Build_ShouldThrow_WhenAppearancesAreNull()
  {
    FluentActions.Invoking(() => TitleRecordsCalculator.Build([], null!)).Should().Throw<ArgumentNullException>();
  }
}

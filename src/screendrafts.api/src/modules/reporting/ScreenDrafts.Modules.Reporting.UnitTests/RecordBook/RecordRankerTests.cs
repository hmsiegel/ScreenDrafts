namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class RecordRankerTests
{
  private sealed record Candidate(string Name, int Drafts, decimal Score);

  private static RecordHolder ToHolder(Candidate c) => new() { Kind = "drafter", Name = c.Name };

  private static RecordItem? Rank(IEnumerable<Candidate> candidates, bool highest) =>
    RecordRanker.Build("code", "Label", RecordRanker.Count, candidates, c => c.Score, highest, ToHolder);

  [Fact]
  public void Build_ShouldReturnHighest_WhenHighestIsTrue()
  {
    var item = Rank([new("A", 1, 3), new("B", 1, 7), new("C", 1, 5)], highest: true);

    item.Should().NotBeNull();
    item.Value.Should().Be(7);
    item.Holders.Select(h => h.Name).Should().Equal("B");
    item.Code.Should().Be("code");
    item.Format.Should().Be(RecordRanker.Count);
  }

  [Fact]
  public void Build_ShouldReturnLowest_WhenHighestIsFalse()
  {
    var item = Rank([new("A", 1, 3), new("B", 1, 7), new("C", 1, 5)], highest: false);

    item!.Value.Should().Be(3);
    item.Holders.Select(h => h.Name).Should().Equal("A");
  }

  [Fact]
  public void Build_ShouldReturnEveryTiedHolderSortedByName_WhenValuesTie()
  {
    var item = Rank([new("Zed", 1, 4), new("amy", 1, 4), new("Bob", 1, 4), new("Low", 1, 1)], highest: true);

    item!.Holders.Select(h => h.Name).Should().Equal("amy", "Bob", "Zed");
  }

  [Fact]
  public void Build_ShouldReturnNull_WhenMostRecordBestValueIsZero()
  {
    Rank([new("A", 1, 0), new("B", 1, 0)], highest: true).Should().BeNull();
  }

  [Fact]
  public void Build_ShouldReturnZeroValue_WhenFewestRecordBestValueIsZero()
  {
    var item = Rank([new("A", 1, 0), new("B", 1, 2)], highest: false);

    item.Should().NotBeNull();
    item.Value.Should().Be(0);
    item.Holders.Select(h => h.Name).Should().Equal("A");
  }

  [Fact]
  public void Build_ShouldReturnNull_WhenThereAreNoCandidates()
  {
    Rank([], highest: true).Should().BeNull();
    Rank([], highest: false).Should().BeNull();
  }

  [Fact]
  public void Build_ShouldRoundValueToFourDecimals()
  {
    var item = Rank([new("A", 1, 1m / 3m)], highest: true);

    item!.Value.Should().Be(0.3333m);
  }

  [Fact]
  public void Build_ShouldCarryQualifierAndFormat_WhenSupplied()
  {
    var item = RecordRanker.Build(
      "code", "Label", RecordRanker.Percent, [new Candidate("A", 1, 5)], c => c.Score, true, ToHolder, "10+ drafts");

    item!.Qualifier.Should().Be("10+ drafts");
    item.Format.Should().Be(RecordRanker.Percent);
  }

  [Fact]
  public void Build_ShouldThrow_WhenArgumentsAreNull()
  {
    var candidates = new List<Candidate>();

    FluentActions.Invoking(() => RecordRanker.Build("c", "l", "count", (IEnumerable<Candidate>)null!, c => c.Score, true, ToHolder))
      .Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => RecordRanker.Build("c", "l", "count", candidates, null!, true, ToHolder))
      .Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => RecordRanker.Build<Candidate>("c", "l", "count", candidates, c => c.Score, true, null!))
      .Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void BuildTiered_ShouldEmitOneRecordPerTierWithMinCodesAndQualifier()
  {
    var candidates = new List<Candidate> { new("Few", 5, 9), new("Many", 20, 3) };

    var items = RecordRanker
      .BuildTiered("rec", "Label", RecordRanker.Count, candidates, c => c.Drafts, c => c.Score, true, ToHolder, [5, 10, 15, 20])
      .ToList();

    items.Select(i => i.Code).Should().Equal("rec.min5", "rec.min10", "rec.min15", "rec.min20");
    items.Select(i => i.Qualifier).Should().Equal("5+ drafts", "10+ drafts", "15+ drafts", "20+ drafts");
    items[0].Holders.Single().Name.Should().Be("Few");
    items[1].Holders.Single().Name.Should().Be("Many");
  }

  [Fact]
  public void BuildTiered_ShouldSkipTiers_WhenNoCandidateQualifies()
  {
    var candidates = new List<Candidate> { new("A", 6, 1) };

    var items = RecordRanker
      .BuildTiered("rec", "Label", RecordRanker.Count, candidates, c => c.Drafts, c => c.Score, true, ToHolder, [5, 10, 15])
      .ToList();

    items.Select(i => i.Code).Should().Equal("rec.min5");
  }

  [Fact]
  public void BuildTiered_ShouldUseOnlyTheSuppliedTierList()
  {
    var candidates = new List<Candidate> { new("A", 30, 1) };

    var items = RecordRanker
      .BuildTiered("rec", "Label", RecordRanker.Count, candidates, c => c.Drafts, c => c.Score, true, ToHolder, [10, 15, 20])
      .ToList();

    items.Select(i => i.Code).Should().Equal("rec.min10", "rec.min15", "rec.min20");
  }

  [Fact]
  public void BuildTiered_ShouldThrow_WhenArgumentsAreNull()
  {
    var candidates = new List<Candidate>();

    FluentActions.Invoking(() => RecordRanker.BuildTiered("c", "l", "count", (IEnumerable<Candidate>)null!, c => c.Drafts, c => c.Score, true, ToHolder, [5]))
      .Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => RecordRanker.BuildTiered("c", "l", "count", candidates, null!, c => c.Score, true, ToHolder, [5]))
      .Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => RecordRanker.BuildTiered("c", "l", "count", candidates, c => c.Drafts, null!, true, ToHolder, [5]))
      .Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => RecordRanker.BuildTiered<Candidate>("c", "l", "count", candidates, c => c.Drafts, c => c.Score, true, null!, [5]))
      .Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => RecordRanker.BuildTiered("c", "l", "count", candidates, c => c.Drafts, c => c.Score, true, ToHolder, null!))
      .Should().Throw<ArgumentNullException>();
  }
}

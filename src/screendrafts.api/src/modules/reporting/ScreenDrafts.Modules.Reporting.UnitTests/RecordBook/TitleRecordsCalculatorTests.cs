using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.RecordBookTestData;

namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class TitleRecordsCalculatorTests
{
  [Fact]
  public void Build_ShouldRankMostTimesDrafted()
  {
    var section = TitleRecordsCalculator.Build([Media(1, timesDrafted: 3), Media(2, timesDrafted: 5), Media(3, timesDrafted: 2)]);

    var item = Find(section, "title.most-times-drafted");

    item!.Value.Should().Be(5);
    item.Holders.Single().Kind.Should().Be("title");
    item.Holders.Single().Name.Should().Be("Movie 2");
    item.Holders.Single().PublicId.Should().Be("m_2");
  }

  [Fact]
  public void Build_ShouldReturnEveryTiedTitleSortedByName_ForMostTimesDraftedAtNumberOne()
  {
    var section = TitleRecordsCalculator.Build([Media(2, 4, 2), Media(1, 4, 2), Media(3, 9, 1)]);

    var item = Find(section, "title.most-times-drafted-no1");

    item!.Value.Should().Be(2);
    item.Holders.Select(h => h.Name).Should().Equal("Movie 1", "Movie 2");
  }

  [Fact]
  public void Build_ShouldOmitNumberOneRecord_WhenNoTitleWasDraftedAtNumberOne()
  {
    var section = TitleRecordsCalculator.Build([Media(1, 4)]);

    Find(section, "title.most-times-drafted-no1").Should().BeNull();
    Find(section, "title.most-times-drafted").Should().NotBeNull();
  }

  [Fact]
  public void Build_ShouldReturnTitleSectionWithNoRecords_WhenThereIsNoMedia()
  {
    var section = TitleRecordsCalculator.Build([]);

    section.Key.Should().Be("title");
    section.Groups.SelectMany(g => g.Records).Should().BeEmpty();
  }

  [Fact]
  public void Build_ShouldThrow_WhenMediaIsNull()
  {
    FluentActions.Invoking(() => TitleRecordsCalculator.Build(null!)).Should().Throw<ArgumentNullException>();
  }
}

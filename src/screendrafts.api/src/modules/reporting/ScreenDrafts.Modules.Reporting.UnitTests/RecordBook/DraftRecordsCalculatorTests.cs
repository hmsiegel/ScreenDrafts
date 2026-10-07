using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.RecordBookTestData;

namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class DraftRecordsCalculatorTests
{
  private static readonly IReadOnlyDictionary<Guid, int> _noUnique = new Dictionary<Guid, int>();
  private static readonly IReadOnlySet<Guid> _noCopacetic = new HashSet<Guid>();

  private static RecordBookSection Build(
    IEnumerable<DraftPartRow> parts,
    IReadOnlyDictionary<Guid, int>? unique = null,
    IReadOnlySet<Guid>? copacetic = null) =>
    DraftRecordsCalculator.Build([.. parts], unique ?? _noUnique, copacetic ?? _noCopacetic);

  [Fact]
  public void Build_ShouldSumPicksLandedAcrossPartsOfADraft()
  {
    var parts = new[]
    {
      Part(1, p => { p.PartIndex = 1; p.PicksLanded = 7; }),
      Part(1, p => { p.PartIndex = 2; p.PicksLanded = 8; }),
      Part(2, p => p.PicksLanded = 10),
    };

    var item = Find(Build(parts), "draft.most-titles-drafted");

    item!.Value.Should().Be(15);
    item.Holders.Single().Name.Should().Be("Draft 1");
    item.Holders.Single().Kind.Should().Be("draft");
    item.Holders.Single().PublicId.Should().Be("d_1");
  }

  [Fact]
  public void Build_ShouldUseSinglePart_ForSingleRecordingRecords_AndNamePartOnlyWhenMultiPart()
  {
    var parts = new[]
    {
      Part(1, p => { p.PartIndex = 1; p.PicksLanded = 7; }),
      Part(1, p => { p.PartIndex = 2; p.PicksLanded = 9; }),
      Part(2, p => p.PicksLanded = 8),
    };

    var multi = Find(Build(parts), "draft.most-titles-drafted-single-recording");

    multi!.Value.Should().Be(9);
    multi.Holders.Single().Context.Should().Be("Part 2");

    var single = Find(Build([Part(2, p => p.PicksLanded = 8)]), "draft.most-titles-drafted-single-recording");

    single!.Holders.Single().Context.Should().BeNull();
  }

  [Fact]
  public void Build_ShouldTakeUniqueTitlesFromTheDictionary_NotTheSumOfParts()
  {
    var parts = new[]
    {
      Part(1, p => { p.PartIndex = 1; p.UniqueTitlesPlayed = 10; }),
      Part(1, p => { p.PartIndex = 2; p.UniqueTitlesPlayed = 10; }),
    };
    var unique = new Dictionary<Guid, int> { [Id(1)] = 15 };

    var section = Build(parts, unique);

    Find(section, "draft.most-unique-titles-played")!.Value.Should().Be(15);
    Find(section, "draft.most-unique-titles-played-single-recording")!.Value.Should().Be(10);
  }

  [Fact]
  public void Build_ShouldTreatMissingDictionaryEntryAsZero()
  {
    var parts = new[] { Part(1, p => p.UniqueTitlesPlayed = 10) };

    Find(Build(parts), "draft.most-unique-titles-played").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldUseOnlyStandardDrafts_ForNonExpandedRecord()
  {
    var parts = new[]
    {
      Part(1, p => { p.DraftType = "Mega"; p.PicksVetoed = 9; }),
      Part(2, p => { p.DraftType = "Standard"; p.PicksVetoed = 4; }),
      Part(3, p => { p.DraftType = "Standard"; p.PicksVetoed = 2; }),
    };

    var section = Build(parts);

    Find(section, "draft.most-picks-vetoed")!.Value.Should().Be(9);
    var nonExpanded = Find(section, "draft.most-picks-vetoed-non-expanded");
    nonExpanded!.Value.Should().Be(4);
    nonExpanded.Holders.Single().Name.Should().Be("Draft 2");
  }

  [Fact]
  public void Build_ShouldOmitNonExpandedRecord_WhenThereAreNoStandardDrafts()
  {
    var parts = new[] { Part(1, p => { p.DraftType = "Mega"; p.PicksVetoed = 3; }) };

    Find(Build(parts), "draft.most-picks-vetoed-non-expanded").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldRankOnlyCopaceticDrafts_ForMostTitlesInACopaceticDraft()
  {
    var parts = new[]
    {
      Part(1, p => p.PicksLanded = 30),
      Part(2, p => p.PicksLanded = 12),
    };

    var item = Find(Build(parts, copacetic: new HashSet<Guid> { Id(2) }), "draft.most-titles-drafted-in-a-copacetic-draft");

    item!.Value.Should().Be(12);
    item.Holders.Single().Name.Should().Be("Draft 2");
  }

  [Fact]
  public void Build_ShouldOmitCopaceticRecord_WhenNoDraftIsCopacetic()
  {
    Find(Build([Part(1, p => p.PicksLanded = 5)]), "draft.most-titles-drafted-in-a-copacetic-draft").Should().BeNull();
  }

  [Fact]
  public void Build_ShouldSumVetoOverridesAndCommissionerOverridesAcrossParts()
  {
    var parts = new[]
    {
      Part(1, p => { p.PartIndex = 1; p.VetoesOverridden = 1; p.CommissionerOverrides = 2; p.No1Vetoed = 1; }),
      Part(1, p => { p.PartIndex = 2; p.VetoesOverridden = 2; p.CommissionerOverrides = 1; p.No1Vetoed = 1; }),
    };

    var section = Build(parts);

    Find(section, "draft.most-vetoes-overridden")!.Value.Should().Be(3);
    Find(section, "draft.most-vetoes-overridden-single-recording")!.Value.Should().Be(2);
    Find(section, "draft.most-commissioner-overrides")!.Value.Should().Be(3);
    Find(section, "draft.most-no1-picks-vetoed")!.Value.Should().Be(2);
  }

  [Fact]
  public void Build_ShouldReturnEveryDraft_WhenValuesTie()
  {
    var parts = new[] { Part(2, p => p.PicksVetoed = 3), Part(1, p => p.PicksVetoed = 3) };

    var item = Find(Build(parts), "draft.most-picks-vetoed");

    item!.Holders.Select(h => h.Name).Should().Equal("Draft 1", "Draft 2");
  }

  [Fact]
  public void Build_ShouldReturnTheDraftSectionWithAllGroups()
  {
    var section = Build([]);

    section.Key.Should().Be("draft");
    section.Groups.Select(g => g.Key).Should().Equal("titles", "vetoes", "veto-overrides", "commissioner-overrides", "copacetic");
    section.Groups.SelectMany(g => g.Records).Should().BeEmpty();
  }

  [Fact]
  public void Build_ShouldThrow_WhenArgumentsAreNull()
  {
    FluentActions.Invoking(() => DraftRecordsCalculator.Build(null!, _noUnique, _noCopacetic)).Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => DraftRecordsCalculator.Build([], null!, _noCopacetic)).Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => DraftRecordsCalculator.Build([], _noUnique, null!)).Should().Throw<ArgumentNullException>();
  }
}

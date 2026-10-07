using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.RecordBookTestData;

namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class RecordBookBuilderTests
{
  private static readonly DateTime _generatedAt = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

  [Fact]
  public void Build_ShouldReturnGuestGmDraftAndTitleSectionsInOrder()
  {
    var response = RecordBookBuilder.Build(Data(), includesNonCanonical: false, _generatedAt);

    response.Sections.Select(s => s.Key).Should().Equal("guest-gm", "draft", "title");
  }

  [Fact]
  public void Build_ShouldPassThroughGeneratedAtAndScopeFlag()
  {
    var canonical = RecordBookBuilder.Build(Data(), includesNonCanonical: false, _generatedAt);
    var all = RecordBookBuilder.Build(Data(), includesNonCanonical: true, _generatedAt);

    canonical.GeneratedAtUtc.Should().Be(_generatedAt);
    canonical.IncludesNonCanonical.Should().BeFalse();
    all.IncludesNonCanonical.Should().BeTrue();
  }

  [Fact]
  public void Build_ShouldDeriveCopaceticTotalFromParts()
  {
    var parts = new[]
    {
      Part(1),
      Part(2, p => p.VetoesIssued = 1),
      Part(3, p => p.PartIndex = 1),
      Part(3, p => { p.PartIndex = 2; p.CommissionerOverrides = 1; }),
    };

    var response = RecordBookBuilder.Build(Data(parts: parts), false, _generatedAt);

    response.Totals.Single(t => t.Code == "copacetic-drafts").Value.Should().Be(1);
    response.Totals.Single(t => t.Code == "drafts").Value.Should().Be(3);
  }

  [Fact]
  public void Build_ShouldFeedTheSameCopaceticSetToDrafterAndDraftRecords()
  {
    var parts = new[] { Part(1, p => p.PicksLanded = 10), Part(2, p => { p.PicksLanded = 20; p.VetoesIssued = 1; }) };
    var rows = new[] { DrafterDraft(1, 1), DrafterDraft(1, 2) };

    var response = RecordBookBuilder.Build(Data(parts: parts, drafterDrafts: rows), false, _generatedAt);

    Find(response.Sections[0], "guest-gm.most-copacetic-drafts")!.Value.Should().Be(1);
    var draftRecord = Find(response.Sections[1], "draft.most-titles-drafted-in-a-copacetic-draft");
    draftRecord!.Holders.Single().Name.Should().Be("Draft 1");
  }

  [Fact]
  public void Build_ShouldTakeUniqueTitlesPerDraftFromTheDictionary()
  {
    var parts = new[]
    {
      Part(1, p => { p.PartIndex = 1; p.UniqueTitlesPlayed = 8; }),
      Part(1, p => { p.PartIndex = 2; p.UniqueTitlesPlayed = 8; }),
    };

    var response = RecordBookBuilder.Build(
      Data(parts: parts, uniqueTitles: new Dictionary<Guid, int> { [Id(1)] = 13 }), false, _generatedAt);

    Find(response.Sections[1], "draft.most-unique-titles-played")!.Value.Should().Be(13);
  }

  [Fact]
  public void Build_ShouldThrow_WhenDataIsNull()
  {
    FluentActions.Invoking(() => RecordBookBuilder.Build(null!, false, _generatedAt)).Should().Throw<ArgumentNullException>();
  }
}

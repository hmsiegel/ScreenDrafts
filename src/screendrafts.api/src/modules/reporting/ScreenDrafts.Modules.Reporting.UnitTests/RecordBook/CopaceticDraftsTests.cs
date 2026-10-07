using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.RecordBookTestData;

namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class CopaceticDraftsTests
{
  [Fact]
  public void Find_ShouldIncludeDraft_WhenNoVetoesAndNoOverrides()
  {
    CopaceticDrafts.Find([Part(1)]).Should().BeEquivalentTo([Id(1)]);
  }

  [Fact]
  public void Find_ShouldExcludeDraft_WhenAnyVetoWasIssuedEvenIfOverridden()
  {
    // VetoesIssued counts every veto row: overridden ones and community vetoes included.
    var parts = new[] { Part(1, p => { p.VetoesIssued = 1; p.VetoesOverridden = 1; }) };

    CopaceticDrafts.Find(parts).Should().BeEmpty();
  }

  [Fact]
  public void Find_ShouldExcludeDraft_WhenACommissionerOverrideExists()
  {
    CopaceticDrafts.Find([Part(1, p => p.CommissionerOverrides = 1)]).Should().BeEmpty();
  }

  [Fact]
  public void Find_ShouldRequireEveryPartToBeClean_WhenDraftHasSeveralParts()
  {
    var parts = new[]
    {
      Part(1, p => p.PartIndex = 1),
      Part(1, p => { p.PartIndex = 2; p.VetoesIssued = 1; }),
      Part(2, p => p.PartIndex = 1),
      Part(2, p => p.PartIndex = 2),
    };

    CopaceticDrafts.Find(parts).Should().BeEquivalentTo([Id(2)]);
  }

  [Fact]
  public void Find_ShouldReturnEmpty_WhenThereAreNoParts()
  {
    CopaceticDrafts.Find([]).Should().BeEmpty();
  }

  [Fact]
  public void Find_ShouldThrow_WhenPartsAreNull()
  {
    FluentActions.Invoking(() => CopaceticDrafts.Find(null!)).Should().Throw<ArgumentNullException>();
  }
}

using static ScreenDrafts.Modules.Reporting.UnitTests.Builders.RecordBookTestData;

namespace ScreenDrafts.Modules.Reporting.UnitTests.RecordBook;

public sealed class RecordBookTotalsCalculatorTests
{
  private static int Total(IReadOnlyList<RecordBookTotal> totals, string code) =>
    totals.Single(t => t.Code == code).Value;

  [Fact]
  public void Build_ShouldCountDistinctDrafts_NotParts()
  {
    var data = Data(parts: [Part(1, p => p.PartIndex = 1), Part(1, p => p.PartIndex = 2), Part(2)]);

    Total(RecordBookTotalsCalculator.Build(data, new HashSet<Guid>()), "drafts").Should().Be(2);
  }

  [Fact]
  public void Build_ShouldSumLandedPicksAndCommissionerOverrides()
  {
    var data = Data(parts:
    [
      Part(1, p => { p.PicksLanded = 7; p.CommissionerOverrides = 1; }),
      Part(2, p => { p.PicksLanded = 8; p.CommissionerOverrides = 2; }),
    ]);

    var totals = RecordBookTotalsCalculator.Build(data, new HashSet<Guid>());

    Total(totals, "picks-made").Should().Be(15);
    Total(totals, "commissioner-overrides").Should().Be(3);
  }

  [Fact]
  public void Build_ShouldCountUniqueTitlesFromMedia_NotFromPickCounts()
  {
    // Three titles, drafted 5, 3 and 1 times: unique titles is the number of media rows.
    var data = Data(media: [Media(1, 5), Media(2, 3), Media(3, 1)]);

    Total(RecordBookTotalsCalculator.Build(data, new HashSet<Guid>()), "unique-titles-drafted").Should().Be(3);
  }

  [Fact]
  public void Build_ShouldReportVetoAndHonorificTotalsFromTheData()
  {
    var data = Data(overrides: o =>
    {
      o.VetoesStood = 12;
      o.VetoesOverridden = 3;
      o.SelfVetoesStood = 2;
      o.Marquee = 40;
      o.HatTrick = 9;
      o.GrandSlam = 2;
    });

    var totals = RecordBookTotalsCalculator.Build(data, new HashSet<Guid>());

    Total(totals, "vetoes-deployed").Should().Be(12);
    Total(totals, "vetoes-overridden").Should().Be(3);
    Total(totals, "self-vetoes").Should().Be(2);
    Total(totals, "marquee-of-fame-titles").Should().Be(40);
    Total(totals, "hat-trick-titles").Should().Be(9);
    Total(totals, "grand-slam-titles").Should().Be(2);
  }

  [Fact]
  public void Build_ShouldCountCopaceticDraftsFromTheSuppliedSet()
  {
    var totals = RecordBookTotalsCalculator.Build(Data(), new HashSet<Guid> { Id(1), Id(2), Id(3) });

    Total(totals, "copacetic-drafts").Should().Be(3);
  }

  [Fact]
  public void Build_ShouldCountUniqueGuestGms_ByDistinctDrafterWhoAppeared()
  {
    var rows = new[]
    {
      DrafterDraft(1, 1),
      DrafterDraft(1, 2),
      DrafterDraft(2, 1),
      DrafterDraft(3, 1, r => r.Appeared = false),
    };

    Total(RecordBookTotalsCalculator.Build(Data(drafterDrafts: rows), new HashSet<Guid>()), "unique-guest-gms").Should().Be(2);
  }

  [Fact]
  public void Build_ShouldEmitAllTotalsInOrder()
  {
    var totals = RecordBookTotalsCalculator.Build(Data(), new HashSet<Guid>());

    totals.Select(t => t.Code).Should().Equal(
      "drafts",
      "picks-made",
      "unique-titles-drafted",
      "vetoes-deployed",
      "vetoes-overridden",
      "commissioner-overrides",
      "copacetic-drafts",
      "self-vetoes",
      "unique-guest-gms",
      "marquee-of-fame-titles",
      "hat-trick-titles",
      "grand-slam-titles");
    totals.Should().OnlyContain(t => t.Value == 0);
  }

  [Fact]
  public void Build_ShouldThrow_WhenArgumentsAreNull()
  {
    FluentActions.Invoking(() => RecordBookTotalsCalculator.Build(null!, new HashSet<Guid>())).Should().Throw<ArgumentNullException>();
    FluentActions.Invoking(() => RecordBookTotalsCalculator.Build(Data(), null!)).Should().Throw<ArgumentNullException>();
  }
}

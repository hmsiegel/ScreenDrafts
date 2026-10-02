using System.Globalization;

namespace ScreenDrafts.Modules.Drafts.UnitTests.WikiExport;

public sealed class WikiTextTests
{
  private static readonly Guid _pickId = Guid.NewGuid();
  private static readonly Guid _alice = Guid.NewGuid();
  private static readonly Guid _bob = Guid.NewGuid();
  private static readonly Guid _carol = Guid.NewGuid();

  private static readonly Dictionary<Guid, string> _names = new()
  {
    [_alice] = "Alice",
    [_bob] = "Bob",
    [_carol] = "Carol",
  };

  private static string NameOf(Guid? id) => id is { } key ? _names[key] : "Unknown";

  private static VetoRow Veto(
    int sequence,
    Guid issuedBy,
    bool isOverridden = false,
    Guid? overriddenBy = null
  ) => new(_pickId, sequence, issuedBy, isOverridden, overriddenBy);

  // -------------------------------------------------------------------------
  // Ordinal
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(1, "1st")]
  [InlineData(2, "2nd")]
  [InlineData(3, "3rd")]
  [InlineData(4, "4th")]
  [InlineData(10, "10th")]
  [InlineData(11, "11th")]
  [InlineData(12, "12th")]
  [InlineData(13, "13th")]
  [InlineData(14, "14th")]
  [InlineData(21, "21st")]
  [InlineData(22, "22nd")]
  [InlineData(23, "23rd")]
  [InlineData(100, "100th")]
  [InlineData(101, "101st")]
  [InlineData(111, "111th")]
  [InlineData(112, "112th")]
  [InlineData(113, "113th")]
  [InlineData(121, "121st")]
  public void Ordinal_ShouldAppendTheEnglishSuffix(int number, string expected) =>
    WikiText.Ordinal(number).Should().Be(expected);

  // -------------------------------------------------------------------------
  // Roman
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(1, "I")]
  [InlineData(2, "II")]
  [InlineData(3, "III")]
  [InlineData(4, "IV")]
  [InlineData(5, "V")]
  [InlineData(6, "VI")]
  [InlineData(7, "VII")]
  [InlineData(8, "VIII")]
  [InlineData(9, "IX")]
  [InlineData(10, "X")]
  public void Roman_ShouldReturnNumerals_ForOneThroughTen(int number, string expected) =>
    WikiText.Roman(number).Should().Be(expected);

  [Theory]
  [InlineData(11, "11")]
  [InlineData(12, "12")]
  [InlineData(50, "50")]
  public void Roman_ShouldFallBackToDigits_AboveTen(int number, string expected) =>
    WikiText.Roman(number).Should().Be(expected);

  // -------------------------------------------------------------------------
  // JoinNames
  // -------------------------------------------------------------------------

  [Fact]
  public void JoinNames_ShouldReturnEmpty_WhenThereAreNoItems() =>
    WikiText.JoinNames([]).Should().BeEmpty();

  [Fact]
  public void JoinNames_ShouldReturnTheItem_WhenThereIsOne() =>
    WikiText.JoinNames(["A"]).Should().Be("A");

  [Fact]
  public void JoinNames_ShouldJoinWithAnd_WhenThereAreTwo() =>
    WikiText.JoinNames(["A", "B"]).Should().Be("A and B");

  [Theory]
  [InlineData("A, B and C", "A", "B", "C")]
  [InlineData("A, B, C and D", "A", "B", "C", "D")]
  public void JoinNames_ShouldCommaSeparateAllButTheLast_WithoutAnOxfordComma(
    string expected,
    params string[] items
  ) => WikiText.JoinNames(items).Should().Be(expected);

  [Fact]
  public void JoinNames_ShouldThrow_WhenItemsIsNull()
  {
    var act = () => WikiText.JoinNames(null!);

    act.Should().Throw<ArgumentNullException>();
  }

  // -------------------------------------------------------------------------
  // PartSuffix
  // -------------------------------------------------------------------------

  [Fact]
  public void PartSuffix_ShouldBeEmpty_WhenAllPartsAreCovered() =>
    WikiText.PartSuffix([1, 2, 3], 3).Should().BeEmpty();

  [Fact]
  public void PartSuffix_ShouldBeEmpty_WhenTheDraftHasASinglePart() =>
    WikiText.PartSuffix([1], 1).Should().BeEmpty();

  [Fact]
  public void PartSuffix_ShouldBeEmpty_WhenNoPartsAreGiven() =>
    WikiText.PartSuffix([], 3).Should().BeEmpty();

  [Fact]
  public void PartSuffix_ShouldUseTheSingularLabel_WhenOnePartIsCovered() =>
    WikiText.PartSuffix([3], 3).Should().Be(" (Part III)");

  [Fact]
  public void PartSuffix_ShouldUseThePluralLabel_WhenSeveralPartsAreCovered() =>
    WikiText.PartSuffix([1, 2], 3).Should().Be(" (Parts I and II)");

  [Fact]
  public void PartSuffix_ShouldListAllButTheLastWithCommas_WhenThreeOfFourPartsAreCovered() =>
    WikiText.PartSuffix([1, 2, 3], 4).Should().Be(" (Parts I, II and III)");

  [Fact]
  public void PartSuffix_ShouldSortAndDeduplicateThePartIndexes() =>
    WikiText.PartSuffix([2, 1, 1], 3).Should().Be(" (Parts I and II)");

  // -------------------------------------------------------------------------
  // FormatDate
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(2024, 11, 1, "November 1, 2024")]
  [InlineData(2025, 3, 5, "March 5, 2025")]
  [InlineData(2023, 12, 31, "December 31, 2023")]
  public void FormatDate_ShouldUseFullMonthNameAndUnpaddedDay(
    int year,
    int month,
    int day,
    string expected
  ) => WikiText.FormatDate(new DateOnly(year, month, day)).Should().Be(expected);

  [Fact]
  public void FormatDate_ShouldIgnoreTheCurrentCulture()
  {
    var original = CultureInfo.CurrentCulture;

    try
    {
      CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

      WikiText.FormatDate(new DateOnly(2024, 11, 1)).Should().Be("November 1, 2024");
    }
    finally
    {
      CultureInfo.CurrentCulture = original;
    }
  }

  // -------------------------------------------------------------------------
  // FirstToken
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData("Matt Singer", "Matt")]
  [InlineData("  Matt   Singer  ", "Matt")]
  [InlineData("Cher", "Cher")]
  [InlineData("William J. Hurt", "William")]
  [InlineData("", "")]
  public void FirstToken_ShouldReturnTheFirstWhitespaceDelimitedWord(
    string displayName,
    string expected
  ) => WikiText.FirstToken(displayName).Should().Be(expected);

  // -------------------------------------------------------------------------
  // Param
  // -------------------------------------------------------------------------

  [Fact]
  public void Param_ShouldPadTheNameTo17Characters_ForAnEmptyValue() =>
    WikiText.Param("title", string.Empty).Should().Be(" | title             = ");

  [Fact]
  public void Param_ShouldAppendTheValue() =>
    WikiText.Param("episodeNumber", "390").Should().Be(" | episodeNumber     = 390");

  [Fact]
  public void Param_ShouldNotTruncateANameLongerThan17Characters() =>
    WikiText
      .Param("aVeryLongParameterName", "x")
      .Should()
      .Be(" | aVeryLongParameterName = x");

  [Fact]
  public void Param_ShouldPadToExactly17Characters_ForAnExactFitName() =>
    WikiText.Param(new string('n', 17), "v").Should().Be($" | {new string('n', 17)} = v");

  // -------------------------------------------------------------------------
  // IsFinallyVetoed
  // -------------------------------------------------------------------------

  [Fact]
  public void IsFinallyVetoed_ShouldBeFalse_WhenThereAreNoVetoes() =>
    WikiText.IsFinallyVetoed([]).Should().BeFalse();

  [Fact]
  public void IsFinallyVetoed_ShouldBeFalse_WhenTheLastVetoWasOverridden() =>
    WikiText
      .IsFinallyVetoed([Veto(1, _alice, isOverridden: true, overriddenBy: _bob)])
      .Should()
      .BeFalse();

  [Fact]
  public void IsFinallyVetoed_ShouldBeTrue_WhenTheLastVetoWasNotOverridden() =>
    WikiText.IsFinallyVetoed([Veto(1, _alice)]).Should().BeTrue();

  [Fact]
  public void IsFinallyVetoed_ShouldOnlyLookAtTheLastVeto()
  {
    var overriddenThenStanding = new[]
    {
      Veto(1, _alice, isOverridden: true, overriddenBy: _bob),
      Veto(2, _carol),
    };
    var standingThenOverridden = new[]
    {
      Veto(1, _alice),
      Veto(2, _carol, isOverridden: true, overriddenBy: _bob),
    };

    WikiText.IsFinallyVetoed(overriddenThenStanding).Should().BeTrue();
    WikiText.IsFinallyVetoed(standingThenOverridden).Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // OverrideChain
  // -------------------------------------------------------------------------

  [Fact]
  public void OverrideChain_ShouldRenderOneOverriddenVeto() =>
    WikiText
      .OverrideChain([Veto(1, _alice, isOverridden: true, overriddenBy: _bob)], NameOf)
      .Should()
      .Be("<s>vetoed by [[Alice]]</s> veto overridden by [[Bob]]");

  [Fact]
  public void OverrideChain_ShouldRenderSeveralOverriddenVetoesInOrderSeparatedBySpaces() =>
    WikiText
      .OverrideChain(
        [
          Veto(1, _alice, isOverridden: true, overriddenBy: _bob),
          Veto(2, _carol, isOverridden: true, overriddenBy: _alice),
        ],
        NameOf
      )
      .Should()
      .Be(
        "<s>vetoed by [[Alice]]</s> veto overridden by [[Bob]] "
          + "<s>vetoed by [[Carol]]</s> veto overridden by [[Alice]]"
      );

  [Fact]
  public void OverrideChain_ShouldBeEmpty_WhenNoVetoWasOverridden() =>
    WikiText.OverrideChain([Veto(1, _alice)], NameOf).Should().BeEmpty();

  [Fact]
  public void OverrideChain_ShouldBeEmpty_WhenThereAreNoVetoes() =>
    WikiText.OverrideChain([], NameOf).Should().BeEmpty();

  [Fact]
  public void OverrideChain_ShouldSkipTheStandingFinalVeto()
  {
    var chain = WikiText.OverrideChain(
      [Veto(1, _alice, isOverridden: true, overriddenBy: _bob), Veto(2, _carol)],
      NameOf
    );

    chain.Should().Be("<s>vetoed by [[Alice]]</s> veto overridden by [[Bob]]");
  }

  [Fact]
  public void OverrideChain_ShouldThrow_WhenAnArgumentIsNull()
  {
    var nullVetoes = () => WikiText.OverrideChain(null!, NameOf);
    var nullNameOf = () => WikiText.OverrideChain([], null!);

    nullVetoes.Should().Throw<ArgumentNullException>();
    nullNameOf.Should().Throw<ArgumentNullException>();
  }
}

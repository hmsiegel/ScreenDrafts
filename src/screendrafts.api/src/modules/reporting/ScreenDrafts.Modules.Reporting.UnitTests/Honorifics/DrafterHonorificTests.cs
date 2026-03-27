namespace ScreenDrafts.Modules.Reporting.UnitTests.Honorifics;

public sealed class DrafterHonorificTests
{
  // -------------------------------------------------------------------------
  // FromAppearanceCount — None (0–4 appearances)
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  [InlineData(4)]
  public void FromAppearanceCount_ShouldReturnNone_WhenCountIsBelowFive(int count)
  {
    DrafterHonorific.FromAppearanceCount(count).Should().Be(DrafterHonorific.None);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — AllStar (5–9 appearances)
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(5)]
  [InlineData(7)]
  [InlineData(9)]
  public void FromAppearanceCount_ShouldReturnAllStar_WhenCountIsBetweenFiveAndNine(int count)
  {
    DrafterHonorific.FromAppearanceCount(count).Should().Be(DrafterHonorific.AllStar);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — HallOfFame (10–14 appearances)
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(10)]
  [InlineData(12)]
  [InlineData(14)]
  public void FromAppearanceCount_ShouldReturnHallOfFame_WhenCountIsBetweenTenAndFourteen(int count)
  {
    DrafterHonorific.FromAppearanceCount(count).Should().Be(DrafterHonorific.HallOfFame);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — Mvp (15–19 appearances)
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(15)]
  [InlineData(17)]
  [InlineData(19)]
  public void FromAppearanceCount_ShouldReturnMvp_WhenCountIsBetweenFifteenAndNineteen(int count)
  {
    DrafterHonorific.FromAppearanceCount(count).Should().Be(DrafterHonorific.Mvp);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — Legend (20+ appearances)
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(20)]
  [InlineData(25)]
  [InlineData(100)]
  public void FromAppearanceCount_ShouldReturnLegend_WhenCountIsTwentyOrMore(int count)
  {
    DrafterHonorific.FromAppearanceCount(count).Should().Be(DrafterHonorific.Legend);
  }

  // -------------------------------------------------------------------------
  // Boundary checks
  // -------------------------------------------------------------------------

  [Fact]
  public void FromAppearanceCount_ShouldReturnAllStar_AtExactBoundaryOfFive()
  {
    DrafterHonorific.FromAppearanceCount(5).Should().Be(DrafterHonorific.AllStar);
  }

  [Fact]
  public void FromAppearanceCount_ShouldReturnHallOfFame_AtExactBoundaryOfTen()
  {
    DrafterHonorific.FromAppearanceCount(10).Should().Be(DrafterHonorific.HallOfFame);
  }

  [Fact]
  public void FromAppearanceCount_ShouldReturnMvp_AtExactBoundaryOfFifteen()
  {
    DrafterHonorific.FromAppearanceCount(15).Should().Be(DrafterHonorific.Mvp);
  }

  [Fact]
  public void FromAppearanceCount_ShouldReturnLegend_AtExactBoundaryOfTwenty()
  {
    DrafterHonorific.FromAppearanceCount(20).Should().Be(DrafterHonorific.Legend);
  }

  // -------------------------------------------------------------------------
  // SmartEnum values
  // -------------------------------------------------------------------------

  [Fact]
  public void DrafterHonorific_NoneHasValueZero()
  {
    DrafterHonorific.None.Value.Should().Be(0);
  }

  [Fact]
  public void DrafterHonorific_AllStarHasValueOne()
  {
    DrafterHonorific.AllStar.Value.Should().Be(1);
  }

  [Fact]
  public void DrafterHonorific_HallOfFameHasValueTwo()
  {
    DrafterHonorific.HallOfFame.Value.Should().Be(2);
  }

  [Fact]
  public void DrafterHonorific_MvpHasValueThree()
  {
    DrafterHonorific.Mvp.Value.Should().Be(3);
  }

  [Fact]
  public void DrafterHonorific_LegendHasValueFour()
  {
    DrafterHonorific.Legend.Value.Should().Be(4);
  }
}

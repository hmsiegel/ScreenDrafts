namespace ScreenDrafts.Modules.Reporting.UnitTests.Honorifics;

public sealed class MovieHonorificTests
{
  // -------------------------------------------------------------------------
  // FromAppearanceCount — None (0–1 pick)
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(0)]
  [InlineData(1)]
  public void FromAppearanceCount_ShouldReturnNone_WhenCountIsOneOrLess(int count)
  {
    MovieHonorific.FromAppearanceCount(count).Should().Be(MovieHonorific.None);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — MarqueeOfFame (2 picks)
  // -------------------------------------------------------------------------

  [Fact]
  public void FromAppearanceCount_ShouldReturnMarqueeOfFame_WhenCountIsTwo()
  {
    MovieHonorific.FromAppearanceCount(2).Should().Be(MovieHonorific.MarqueeOfFame);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — HatTrick (3 picks)
  // -------------------------------------------------------------------------

  [Fact]
  public void FromAppearanceCount_ShouldReturnHatTrick_WhenCountIsThree()
  {
    MovieHonorific.FromAppearanceCount(3).Should().Be(MovieHonorific.HatTrick);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — GrandSlam (4 picks)
  // -------------------------------------------------------------------------

  [Fact]
  public void FromAppearanceCount_ShouldReturnGrandSlam_WhenCountIsFour()
  {
    MovieHonorific.FromAppearanceCount(4).Should().Be(MovieHonorific.GrandSlam);
  }

  // -------------------------------------------------------------------------
  // FromAppearanceCount — HighFive (5+ picks)
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(100)]
  public void FromAppearanceCount_ShouldReturnHighFive_WhenCountIsFiveOrMore(int count)
  {
    MovieHonorific.FromAppearanceCount(count).Should().Be(MovieHonorific.HighFive);
  }

  // -------------------------------------------------------------------------
  // SmartEnum values
  // -------------------------------------------------------------------------

  [Fact]
  public void MovieHonorific_NoneHasValueZero()
  {
    MovieHonorific.None.Value.Should().Be(0);
  }

  [Fact]
  public void MovieHonorific_MarqueeOfFameHasValueOne()
  {
    MovieHonorific.MarqueeOfFame.Value.Should().Be(1);
  }

  [Fact]
  public void MovieHonorific_HatTrickHasValueTwo()
  {
    MovieHonorific.HatTrick.Value.Should().Be(2);
  }

  [Fact]
  public void MovieHonorific_GrandSlamHasValueThree()
  {
    MovieHonorific.GrandSlam.Value.Should().Be(3);
  }

  [Fact]
  public void MovieHonorific_HighFiveHasValueFour()
  {
    MovieHonorific.HighFive.Value.Should().Be(4);
  }

  // -------------------------------------------------------------------------
  // MoviePositionHonorific flags
  // -------------------------------------------------------------------------

  [Fact]
  public void MoviePositionHonorific_NoneIsZero()
  {
    ((int)MoviePositionHonorific.None).Should().Be(0);
  }

  [Fact]
  public void MoviePositionHonorific_UnifiedNumber1IsOne()
  {
    ((int)MoviePositionHonorific.UnifiedNumber1).Should().Be(1);
  }

  [Fact]
  public void MoviePositionHonorific_TheCycleIsTwo()
  {
    ((int)MoviePositionHonorific.TheCycle).Should().Be(2);
  }

  [Fact]
  public void MoviePositionHonorific_CanCombineFlags()
  {
    var both = MoviePositionHonorific.UnifiedNumber1 | MoviePositionHonorific.TheCycle;
    both.HasFlag(MoviePositionHonorific.UnifiedNumber1).Should().BeTrue();
    both.HasFlag(MoviePositionHonorific.TheCycle).Should().BeTrue();
    ((int)both).Should().Be(3);
  }
}

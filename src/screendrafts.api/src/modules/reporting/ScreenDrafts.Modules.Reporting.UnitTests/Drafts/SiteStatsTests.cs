namespace ScreenDrafts.Modules.Reporting.UnitTests.Drafts;

public sealed class SiteStatsTests
{
  // -------------------------------------------------------------------------
  // Create — initial state
  // -------------------------------------------------------------------------

  [Fact]
  public void Create_ShouldInitializeVetoesCountToZero()
  {
    var stats = SiteStats.Create();

    stats.VetoesCount.Should().Be(0);
  }

  [Fact]
  public void Create_ShouldSetUpdatedAt()
  {
    var before = DateTime.UtcNow;

    var stats = SiteStats.Create();

    stats.UpdatedAt.Should().BeOnOrAfter(before);
  }

  // -------------------------------------------------------------------------
  // IncrementVetoes
  // -------------------------------------------------------------------------

  [Fact]
  public void IncrementVetoes_ShouldAddCountToVetoesCount()
  {
    var stats = SiteStats.Create();

    stats.IncrementVetoes(3);

    stats.VetoesCount.Should().Be(3);
  }

  [Fact]
  public void IncrementVetoes_ShouldAccumulateAcrossCalls()
  {
    var stats = SiteStats.Create();

    stats.IncrementVetoes(2);
    stats.IncrementVetoes(5);

    stats.VetoesCount.Should().Be(7);
  }

  [Fact]
  public void IncrementVetoes_ShouldUpdateUpdatedAt()
  {
    var stats = SiteStats.Create();
    var before = DateTime.UtcNow;

    stats.IncrementVetoes(1);

    stats.UpdatedAt.Should().BeOnOrAfter(before);
  }

  [Theory]
  [InlineData(1)]
  [InlineData(10)]
  [InlineData(100)]
  public void IncrementVetoes_ShouldIncrementByGivenCount(int count)
  {
    var stats = SiteStats.Create();

    stats.IncrementVetoes(count);

    stats.VetoesCount.Should().Be(count);
  }
}

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

public sealed class GetSiteStatsTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  // -------------------------------------------------------------------------
  // No data — defaults to zero
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnZeroForAllStats_WhenNoDataExistsAsync()
  {
    // Act
    var result = await Sender.Send(
      new GetSiteStatsQuery { IsPatreonMember = false },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.EpisodesProduced.Should().Be(0);
    result.Value.FilmsDrafted.Should().Be(0);
    result.Value.GuestGMs.Should().Be(0);
    result.Value.VetoesDeployed.Should().Be(0);
    result.Value.Legends.Should().Be(0);
  }

  // -------------------------------------------------------------------------
  // Public episode count — from draft_part_releases with MainFeed channel
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnPublicEpisodeCount_WhenReleasesExistAsync()
  {
    // Arrange — two MainFeed releases for different parts
    var draftId = Guid.NewGuid();
    await SeedDraftPartReleaseAsync(draftId, _faker.Random.AlphaNumeric(10), "MainFeed");
    await SeedDraftPartReleaseAsync(draftId, _faker.Random.AlphaNumeric(10), "MainFeed");

    // Act
    var result = await Sender.Send(
      new GetSiteStatsQuery { IsPatreonMember = false },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.EpisodesProduced.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Patreon episode count — total draft summaries
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnAllSummariesCount_WhenIsPatreonMemberIsTrueAsync()
  {
    // Arrange — three summaries (mix of Patreon and public)
    var draftId = Guid.NewGuid();
    await SeedDraftSummaryAsync(draftId, _faker.Random.AlphaNumeric(10), isPatreon: false);
    await SeedDraftSummaryAsync(draftId, _faker.Random.AlphaNumeric(10), isPatreon: true);
    await SeedDraftSummaryAsync(draftId, _faker.Random.AlphaNumeric(10), isPatreon: true);

    // Act
    var result = await Sender.Send(
      new GetSiteStatsQuery { IsPatreonMember = true },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.EpisodesProduced.Should().Be(3);
  }

  // -------------------------------------------------------------------------
  // Patreon count excludes Patreon releases from public count
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldExcludePatreonReleases_WhenIsPatreonMemberIsFalseAsync()
  {
    // Arrange — one MainFeed and one Patreon release
    var draftId = Guid.NewGuid();
    await SeedDraftPartReleaseAsync(draftId, _faker.Random.AlphaNumeric(10), "MainFeed");
    await SeedDraftPartReleaseAsync(draftId, _faker.Random.AlphaNumeric(10), "Patreon");

    // Act
    var result = await Sender.Send(
      new GetSiteStatsQuery { IsPatreonMember = false },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.EpisodesProduced.Should().Be(1, "only MainFeed releases count for public members");
  }

  // -------------------------------------------------------------------------
  // Vetoes deployed — from site_stats table
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnVetoesDeployed_WhenSiteStatsHaveVetoesAsync()
  {
    // Arrange — seed SiteStats with 5 vetoes
    var siteStats = SiteStats.Create();
    siteStats.IncrementVetoes(5);
    DbContext.SiteStats.Add(siteStats);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(
      new GetSiteStatsQuery { IsPatreonMember = false },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.VetoesDeployed.Should().Be(5);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task SeedDraftPartReleaseAsync(Guid draftId, string draftPartPublicId, string releaseChannel)
  {
    var release = DraftPartRelease.Create(
      draftId: draftId,
      draftPartPublicId: draftPartPublicId,
      releaseChannel: releaseChannel,
      releaseDate: DateOnly.FromDateTime(DateTime.UtcNow)
    );
    DbContext.DraftPartsReleases.Add(release);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  private async Task SeedDraftSummaryAsync(Guid draftId, string draftPartPublicId, bool isPatreon)
  {
    var summary = DraftSummary.Create(
      draftId: draftId,
      draftPublicId: _faker.Random.AlphaNumeric(10),
      draftPartPublicId: draftPartPublicId,
      title: _faker.Company.CompanyName(),
      draftType: "Standard",
      partIndex: 1,
      totalParts: 1,
      totalPicks: 7,
      isPatreon: isPatreon,
      episodeNumber: null,
      isComplete: false,
      completedAtUtc: null,
      createdAtUtc: DateTime.UtcNow
    );
    DbContext.DraftSummaries.Add(summary);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
  }
}

namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

public sealed class UpsertDraftSummaryTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  // -------------------------------------------------------------------------
  // Create — no existing summary
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldCreateNewDraftSummary_WhenNoneExistsAsync()
  {
    // Arrange
    var command = BuildCommand();

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var summary = await DbContext.DraftSummaries
      .FirstOrDefaultAsync(
        s => s.DraftId == command.DraftId && s.DraftPartPublicId == command.DraftPartPublicId,
        TestContext.Current.CancellationToken
      );

    summary.Should().NotBeNull();
    summary!.Title.Should().Be(command.Title);
    summary.TotalParts.Should().Be(command.TotalParts);
    summary.TotalPicks.Should().Be(command.TotalPicks);
    summary.IsPatreon.Should().Be(command.IsPatreon);
    summary.IsComplete.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Update — existing summary
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldUpdateExistingSummary_WhenSummaryAlreadyExistsAsync()
  {
    // Arrange
    var draftId = Guid.NewGuid();
    var draftPublicId = _faker.Random.AlphaNumeric(10);
    var draftPartPublicId = _faker.Random.AlphaNumeric(10);

    var initialCommand = BuildCommand(draftId, draftPublicId, draftPartPublicId, totalParts: 1, totalPicks: 5);
    await Sender.Send(initialCommand, TestContext.Current.CancellationToken);

    var updateCommand = BuildCommand(draftId, draftPublicId, draftPartPublicId, totalParts: 2, totalPicks: 14);

    // Act
    var result = await Sender.Send(updateCommand, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var summaries = await DbContext.DraftSummaries
      .Where(s => s.DraftId == draftId && s.DraftPartPublicId == draftPartPublicId)
      .ToListAsync(TestContext.Current.CancellationToken);

    summaries.Should().HaveCount(1, "no duplicate should be created");
    summaries[0].TotalParts.Should().Be(2);
    summaries[0].TotalPicks.Should().Be(14);
  }

  // -------------------------------------------------------------------------
  // Idempotent — same data twice
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldBeIdempotent_WhenSameCommandSentTwiceAsync()
  {
    // Arrange
    var command = BuildCommand();

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var count = await DbContext.DraftSummaries
      .CountAsync(
        s => s.DraftId == command.DraftId && s.DraftPartPublicId == command.DraftPartPublicId,
        TestContext.Current.CancellationToken
      );

    count.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // SiteStats veto increment
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldIncrementSiteStatsVetoes_WhenVetoCountIsGreaterThanZeroAsync()
  {
    // Arrange — seed a SiteStats row
    var siteStats = SiteStats.Create();
    DbContext.SiteStats.Add(siteStats);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var command = BuildCommand(vetoCount: 3);

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    await DbContext.Entry(siteStats).ReloadAsync(TestContext.Current.CancellationToken);
    siteStats.VetoesCount.Should().Be(3);
  }

  [Fact]
  public async Task Handle_ShouldNotIncrementSiteStats_WhenVetoCountIsZeroAsync()
  {
    // Arrange
    var siteStats = SiteStats.Create();
    DbContext.SiteStats.Add(siteStats);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var command = BuildCommand(vetoCount: 0);

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    await DbContext.Entry(siteStats).ReloadAsync(TestContext.Current.CancellationToken);
    siteStats.VetoesCount.Should().Be(0);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static UpsertDraftSummaryCommand BuildCommand(
    Guid? draftId = null,
    string? draftPublicId = null,
    string? draftPartPublicId = null,
    int totalParts = 1,
    int totalPicks = 7,
    bool isPatreon = false,
    int vetoCount = 0
  ) =>
    new()
    {
      DraftId = draftId ?? Guid.NewGuid(),
      DraftPublicId = draftPublicId ?? _faker.Random.AlphaNumeric(10),
      DraftPartPublicId = draftPartPublicId ?? _faker.Random.AlphaNumeric(10),
      Title = _faker.Company.CompanyName(),
      DraftType = "Standard",
      PartIndex = 1,
      TotalParts = totalParts,
      TotalPicks = totalPicks,
      IsPatreon = isPatreon,
      EpisodeNumber = null,
      VetoCount = vetoCount,
    };
}

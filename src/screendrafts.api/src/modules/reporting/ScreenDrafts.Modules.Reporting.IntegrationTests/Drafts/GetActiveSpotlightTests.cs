namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

public sealed class GetActiveSpotlightTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  // -------------------------------------------------------------------------
  // No active spotlight
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnFailure_WhenNoActiveSpotlightExistsAsync()
  {
    // Act
    var result = await Sender.Send(
      new GetActiveSpotlightQuery(),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Active spotlight found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnSpotlightData_WhenActiveSpotlightExistsAsync()
  {
    // Arrange — seed a complete, public summary and an active spotlight
    const string description = "A truly legendary episode";
    var draftPublicId = await SeedActiveSpotlightAsync(description: description);

    // Act
    var result = await Sender.Send(
      new GetActiveSpotlightQuery(),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DraftPublicId.Should().Be(draftPublicId);
    result.Value.SpotlightDescription.Should().Be(description);
  }

  [Fact]
  public async Task Handle_ShouldReturnCorrectMetadata_WhenActiveSpotlightExistsAsync()
  {
    // Arrange
    await SeedActiveSpotlightAsync(
      title: "Best Draft Ever",
      episodeNumber: 77,
      totalParts: 2,
      totalPicks: 14
    );

    // Act
    var result = await Sender.Send(
      new GetActiveSpotlightQuery(),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Title.Should().Be("Best Draft Ever");
    result.Value.EpisodeNumber.Should().Be(77);
    result.Value.TotalParts.Should().Be(2);
    result.Value.TotalPicks.Should().Be(14);
  }

  [Fact]
  public async Task Handle_ShouldReturnEmptyTopPicks_WhenNoCanonicalPicksExistAsync()
  {
    // Arrange
    await SeedActiveSpotlightAsync();

    // Act
    var result = await Sender.Send(
      new GetActiveSpotlightQuery(),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TopPicks.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Inactive spotlight is ignored
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnFailure_WhenSpotlightExistsButIsNotActiveAsync()
  {
    // Arrange — create spotlight without activating it
    var draftPublicId = _faker.Random.AlphaNumeric(10);
    var spotlightPublicId = _faker.Random.AlphaNumeric(10);
    await SeedCompletedSummaryAsync(draftPublicId);

    var spotlight = DraftSpotlight.Create(spotlightPublicId, draftPublicId, "Inactive", null);
    DbContext.DraftSpotlights.Add(spotlight);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(
      new GetActiveSpotlightQuery(),
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue("an inactive spotlight should not be returned");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<string> SeedActiveSpotlightAsync(
    string? spotlightPublicId = null,
    string? draftPublicId = null,
    string description = "Great episode",
    string? title = null,
    int? episodeNumber = null,
    int totalParts = 1,
    int totalPicks = 7
  )
  {
    spotlightPublicId ??= _faker.Random.AlphaNumeric(10);
    draftPublicId ??= _faker.Random.AlphaNumeric(10);
    title ??= _faker.Company.CompanyName();

    await SeedCompletedSummaryAsync(
      draftPublicId,
      title: title,
      episodeNumber: episodeNumber,
      totalParts: totalParts,
      totalPicks: totalPicks
    );

    var spotlight = DraftSpotlight.Create(spotlightPublicId, draftPublicId, description, null);
    spotlight.Activate();
    DbContext.DraftSpotlights.Add(spotlight);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    return draftPublicId;
  }

  private async Task SeedCompletedSummaryAsync(
    string draftPublicId,
    string? title = null,
    int? episodeNumber = null,
    int totalParts = 1,
    int totalPicks = 7
  )
  {
    var summary = DraftSummary.Create(
      draftId: Guid.NewGuid(),
      draftPublicId: draftPublicId,
      draftPartPublicId: _faker.Random.AlphaNumeric(10),
      title: title ?? _faker.Company.CompanyName(),
      draftType: "Standard",
      partIndex: 1,
      totalParts: totalParts,
      totalPicks: totalPicks,
      isPatreon: false,
      episodeNumber: episodeNumber,
      isComplete: true,
      completedAtUtc: DateTime.UtcNow.AddDays(-1),
      createdAtUtc: DateTime.UtcNow.AddDays(-2)
    );

    DbContext.DraftSummaries.Add(summary);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
  }
}

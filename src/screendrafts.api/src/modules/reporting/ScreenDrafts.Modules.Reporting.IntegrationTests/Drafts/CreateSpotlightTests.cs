namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

public sealed class CreateSpotlightTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  // -------------------------------------------------------------------------
  // Happy path — complete, non-Patreon draft
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldCreateSpotlight_WhenDraftIsCompleteAndPublicAsync()
  {
    // Arrange
    var draftPublicId = await SeedCompletedDraftSummaryAsync(isPatreon: false);

    var command = new CreateSpotlightCommand
    {
      DraftPublicId = draftPublicId,
      SpotlightDescription = "A stellar episode",
      SpotifyUrl = null,
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.SpotlightId.Should().NotBeEmpty();

    var spotlight = await DbContext.DraftSpotlights
      .FirstOrDefaultAsync(
        s => s.DraftPublicId == draftPublicId,
        TestContext.Current.CancellationToken
      );

    spotlight.Should().NotBeNull();
    spotlight!.SpotlightDescription.Should().Be("A stellar episode");
    spotlight.IsActive.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Failure — draft not found in reporting
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnNotFound_WhenDraftDoesNotExistInReportingAsync()
  {
    // Arrange — valid format but non-existent ID
    var command = new CreateSpotlightCommand
    {
      DraftPublicId = ValidDraftId(),
      SpotlightDescription = "Should fail",
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Failure — draft not yet complete
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnFailure_WhenDraftIsNotCompleteAsync()
  {
    // Arrange — insert an incomplete summary
    var draftPublicId = ValidDraftId();
    var summary = DraftSummary.Create(
      draftId: Guid.NewGuid(),
      draftPublicId: draftPublicId,
      draftPartPublicId: _faker.Random.AlphaNumeric(10),
      title: "Incomplete Draft",
      draftType: "Standard",
      partIndex: 1,
      totalParts: 1,
      totalPicks: 7,
      isPatreon: false,
      episodeNumber: null,
      isComplete: false,
      completedAtUtc: null,
      createdAtUtc: DateTime.UtcNow
    );
    DbContext.DraftSummaries.Add(summary);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var command = new CreateSpotlightCommand
    {
      DraftPublicId = draftPublicId,
      SpotlightDescription = "Too soon",
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Failure — Patreon draft cannot be spotlighted
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnFailure_WhenDraftIsPatreonAsync()
  {
    // Arrange
    var draftPublicId = await SeedCompletedDraftSummaryAsync(isPatreon: true);

    var command = new CreateSpotlightCommand
    {
      DraftPublicId = draftPublicId,
      SpotlightDescription = "Patreon-only draft",
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static string ValidDraftId() => $"d_{_faker.Random.AlphaNumeric(21)}";

  private async Task<string> SeedCompletedDraftSummaryAsync(bool isPatreon)
  {
    var draftPublicId = ValidDraftId();

    var summary = DraftSummary.Create(
      draftId: Guid.NewGuid(),
      draftPublicId: draftPublicId,
      draftPartPublicId: _faker.Random.AlphaNumeric(10),
      title: _faker.Company.CompanyName(),
      draftType: "Standard",
      partIndex: 1,
      totalParts: 1,
      totalPicks: 7,
      isPatreon: isPatreon,
      episodeNumber: 42,
      isComplete: true,
      completedAtUtc: DateTime.UtcNow.AddDays(-1),
      createdAtUtc: DateTime.UtcNow.AddDays(-2)
    );

    DbContext.DraftSummaries.Add(summary);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    return draftPublicId;
  }
}

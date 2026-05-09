namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

public sealed class MarkDraftCompleteTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  // -------------------------------------------------------------------------
  // Happy path — mark all parts complete
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldMarkAllDraftSummariesComplete_WhenSummariesExistAsync()
  {
    // Arrange — two parts for the same draft
    var draftId = Guid.NewGuid();
    var draftPublicId = _faker.Random.AlphaNumeric(10);

    await Sender.Send(BuildUpsertCommand(draftId, draftPublicId, _faker.Random.AlphaNumeric(10), partIndex: 1), TestContext.Current.CancellationToken);
    await Sender.Send(BuildUpsertCommand(draftId, draftPublicId, _faker.Random.AlphaNumeric(10), partIndex: 2), TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(
      new MarkDraftCompleteCommand { DraftId = draftId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();

    var summaries = await DbContext.DraftSummaries
      .Where(s => s.DraftId == draftId)
      .ToListAsync(TestContext.Current.CancellationToken);

    summaries.Should().HaveCount(2);
    summaries.Should().AllSatisfy(s => s.IsComplete.Should().BeTrue());
    summaries.Should().AllSatisfy(s => s.CompletedAtUtc.Should().NotBeNull());
  }

  // -------------------------------------------------------------------------
  // Failure — no summaries found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnFailure_WhenNoDraftSummariesExistAsync()
  {
    // Arrange
    var command = new MarkDraftCompleteCommand { DraftId = Guid.NewGuid() };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Single-part draft
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldMarkSinglePartComplete_WhenDraftHasOnePartAsync()
  {
    // Arrange
    var draftId = Guid.NewGuid();
    var draftPublicId = _faker.Random.AlphaNumeric(10);
    var draftPartPublicId = _faker.Random.AlphaNumeric(10);

    await Sender.Send(
      BuildUpsertCommand(draftId, draftPublicId, draftPartPublicId, partIndex: 1),
      TestContext.Current.CancellationToken
    );

    // Act
    var result = await Sender.Send(
      new MarkDraftCompleteCommand { DraftId = draftId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();

    var summary = await DbContext.DraftSummaries
      .FirstOrDefaultAsync(
        s => s.DraftId == draftId && s.DraftPartPublicId == draftPartPublicId,
        TestContext.Current.CancellationToken
      );

    summary.Should().NotBeNull();
    summary!.IsComplete.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private static UpsertDraftSummaryCommand BuildUpsertCommand(
    Guid draftId,
    string draftPublicId,
    string draftPartPublicId,
    int partIndex
  ) =>
    new()
    {
      DraftId = draftId,
      DraftPublicId = draftPublicId,
      DraftPartPublicId = draftPartPublicId,
      Title = _faker.Company.CompanyName(),
      DraftType = "Standard",
      PartIndex = partIndex,
      TotalParts = 1,
      TotalPicks = 7,
      IsPatreon = false,
      EpisodeNumber = null,
      VetoCount = 0,
    };
}

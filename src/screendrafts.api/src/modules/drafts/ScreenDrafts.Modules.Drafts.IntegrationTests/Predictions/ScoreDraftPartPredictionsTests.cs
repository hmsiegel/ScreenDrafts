namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class ScoreDraftPartPredictionsTests(DraftsIntegrationTestWebAppFactory factory)
  : PredictionIntegrationTestBase(factory)
{
  [Fact]
  public async Task ScoreDraftPartPredictions_WithLockedSet_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, _) = await CreateDraftPartWithLockedSetAsync("m_00000001", "m_00000002", "m_00000003");

    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ScoreDraftPartPredictions_ShouldPersistResult_ToDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, setPublicId) = await CreateDraftPartWithLockedSetAsync("m_00000001", "m_00000002", "m_00000003");

    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var set = await DbContext.DraftPredictionSets.FirstAsync(s => s.PublicId == setPublicId, TestContext.Current.CancellationToken);
    var predictionResult = await DbContext.PredictionResults
      .FirstOrDefaultAsync(r => r.SetId == set.Id, TestContext.Current.CancellationToken);

    predictionResult.Should().NotBeNull();
    predictionResult!.CorrectCount.Should().Be(3);
    predictionResult.ShootTheMoon.Should().BeTrue();
    predictionResult.PointsAwarded.Should().Be(6); // 3 × 2 for shoot-the-moon
  }

  [Fact]
  public async Task ScoreDraftPartPredictions_WithUnlockedSet_ShouldAutoLockAndSucceedAsync()
  {
    // Arrange — submit but do NOT lock
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);
    await SubmitSetAsync(draftPartPublicId, seasonPublicId, contestantPublicId,
      "m_00000001", "m_00000002", "m_00000003");

    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ScoreDraftPartPredictions_WhenAlreadyScored_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _) = await CreateDraftPartWithLockedSetAsync("m_00000001", "m_00000002", "m_00000003");

    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    };

    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — score again
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.AlreadyScored");
  }

  [Fact]
  public async Task ScoreDraftPartPredictions_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = "dp_nonexistent123",
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task ScoreDraftPartPredictions_WithNoSets_ShouldSucceedGracefullyAsync()
  {
    // Arrange — create draft part and rules but no sets
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert — no sets to score, so success
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ScoreDraftPartPredictions_ShouldUpdateStandings_AfterProcessingOutboxAsync()
  {
    // Arrange
    var (draftPartPublicId, setPublicId) = await CreateDraftPartWithLockedSetAsync("m_00000001", "m_00000002", "m_00000003");

    var set = await DbContext.DraftPredictionSets.FirstAsync(s => s.PublicId == setPublicId, TestContext.Current.CancellationToken);
    var seasonId = set.SeasonId;
    var contestantId = set.ContestantId;

    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    };

    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — process the outbox to trigger PredictionSetScoredDomainEventHandler
    await ProcessOutboxAsync();

    // Assert — standing should have been created and updated
    var standing = await DbContext.PredictionStandings
      .FirstOrDefaultAsync(s => s.SeasonId == seasonId && s.ContestantId == contestantId, TestContext.Current.CancellationToken);

    standing.Should().NotBeNull();
    standing!.Points.Should().Be(6); // 3 correct × 2 for shoot-the-moon
  }

  [Fact]
  public async Task ScoreDraftPartPredictions_WithPartialMatch_ShouldAwardCorrectPointsAsync()
  {
    // Arrange — predict 3 movies but only 1 is in the final list
    var (draftPartPublicId, setPublicId) = await CreateDraftPartWithLockedSetAsync("m_00000001", "m_00000002", "m_00000003");

    var command = new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000099", "m_00000098"] // only m_001 matches
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var set = await DbContext.DraftPredictionSets.FirstAsync(s => s.PublicId == setPublicId, TestContext.Current.CancellationToken);
    var predictionResult = await DbContext.PredictionResults
      .FirstOrDefaultAsync(r => r.SetId == set.Id, TestContext.Current.CancellationToken);

    predictionResult.Should().NotBeNull();
    predictionResult!.CorrectCount.Should().Be(1);
    predictionResult.ShootTheMoon.Should().BeFalse();
    predictionResult.PointsAwarded.Should().Be(1);
  }

  // ──────────────────────────────────────────────────────────────────────
  // Helpers
  // ──────────────────────────────────────────────────────────────────────

  private async Task<(string DraftPartPublicId, string SetPublicId)> CreateDraftPartWithLockedSetAsync(
    params string[] mediaPublicIds)
  {
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: mediaPublicIds.Length);
    var setPublicId = await SubmitSetAsync(
      draftPartPublicId, seasonPublicId, contestantPublicId,
      mediaPublicIds);

    var lockResult = await Sender.Send(new LockPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SetPublicId = setPublicId
    }, TestContext.Current.CancellationToken);
    lockResult.IsSuccess.Should().BeTrue("set must be locked before scoring");

    return (draftPartPublicId, setPublicId);
  }
}

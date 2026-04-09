namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class GetDraftPartPredictionsTests(DraftsIntegrationTestWebAppFactory factory)
  : PredictionIntegrationTestBase(factory)
{
  [Fact]
  public async Task GetDraftPartPredictions_WithNoSets_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var query = new GetDraftPartPredictionsQuery { DraftPartPublicId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraftPartPredictions_WithOneSet_ShouldReturnSetWithEntriesAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);
    await SubmitSetAsync(draftPartPublicId, seasonPublicId, contestantPublicId,
      "m_00000001", "m_00000002", "m_00000003");

    var query = new GetDraftPartPredictionsQuery { DraftPartPublicId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(1);
    result.Value[0].ContestantPublicId.Should().Be(contestantPublicId);
    result.Value[0].Entries.Should().HaveCount(3);
    result.Value[0].IsLocked.Should().BeFalse();
    result.Value[0].Result.Should().BeNull();
  }

  [Fact]
  public async Task GetDraftPartPredictions_WithLockedSet_ShouldShowIsLockedTrueAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);
    var setPublicId = await SubmitSetAsync(draftPartPublicId, seasonPublicId, contestantPublicId,
      "m_00000001", "m_00000002", "m_00000003");

    await Sender.Send(new LockPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SetPublicId = setPublicId
    });

    var query = new GetDraftPartPredictionsQuery { DraftPartPublicId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(1);
    result.Value[0].IsLocked.Should().BeTrue();
    result.Value[0].LockedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public async Task GetDraftPartPredictions_WithScoredSet_ShouldIncludeResultAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);
    var setPublicId = await SubmitSetAsync(draftPartPublicId, seasonPublicId, contestantPublicId,
      "m_00000001", "m_00000002", "m_00000003");

    await Sender.Send(new LockPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SetPublicId = setPublicId
    });

    await Sender.Send(new ScoreDraftPartPredictionsCommand
    {
      DraftPartPublicId = draftPartPublicId,
      FinalMediaPublicIds = ["m_00000001", "m_00000002", "m_00000003"]
    });

    var query = new GetDraftPartPredictionsQuery { DraftPartPublicId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(1);
    result.Value[0].Result.Should().NotBeNull();
    result.Value[0].Result!.CorrectCount.Should().Be(3);
    result.Value[0].Result!.ShootsTheMoon.Should().BeTrue();
    result.Value[0].Result!.PointsAwarded.Should().Be(6);
  }

  [Fact]
  public async Task GetDraftPartPredictions_WithMultipleSets_ShouldReturnAllSetsAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId1 = await CreateContestantPublicIdAsync();
    var contestantPublicId2 = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    await SubmitSetAsync(draftPartPublicId, seasonPublicId, contestantPublicId1,
      "m_00000001", "m_00000002", "m_00000003");
    await SubmitSetAsync(draftPartPublicId, seasonPublicId, contestantPublicId2,
      "m_00000004", "m_00000005", "m_00000006");

    var query = new GetDraftPartPredictionsQuery { DraftPartPublicId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().HaveCount(2);
  }
}

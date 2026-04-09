namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class LockPredictionSetTests(DraftsIntegrationTestWebAppFactory factory)
  : PredictionIntegrationTestBase(factory)
{
  [Fact]
  public async Task LockPredictionSet_WithValidSet_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, setPublicId) = await CreateDraftPartWithSetAsync();

    var command = new LockPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SetPublicId = setPublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task LockPredictionSet_ShouldSetIsLockedAndCaptureSnapshot_InDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, setPublicId) = await CreateDraftPartWithSetAsync();

    var command = new LockPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SetPublicId = setPublicId
    };

    // Act
    await Sender.Send(command);

    // Assert
    var set = await DbContext.DraftPredictionSets
      .FirstAsync(s => s.PublicId == setPublicId);
    set.IsLocked.Should().BeTrue();
    set.LockedAtUtc.Should().NotBeNull();
    set.RulesSnapshot.Should().NotBeNull();
  }

  [Fact]
  public async Task LockPredictionSet_WhenAlreadyLocked_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, setPublicId) = await CreateDraftPartWithSetAsync();

    var command = new LockPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SetPublicId = setPublicId
    };

    await Sender.Send(command);

    // Act — attempt to lock again
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.SetAlreadyLocked);
  }

  [Fact]
  public async Task LockPredictionSet_WithNonExistentSet_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var command = new LockPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SetPublicId = "set_nonexistent123"
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task LockPredictionSet_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);
    var setPublicId = await SubmitSetAsync(draftPartPublicId, seasonPublicId, contestantPublicId, "m_00000001", "m_00000002", "m_00000003");

    var command = new LockPredictionSetCommand
    {
      DraftPartPublicId = "dp_nonexistent123",
      SetPublicId = setPublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ──────────────────────────────────────────────────────────────────────
  // Helpers
  // ──────────────────────────────────────────────────────────────────────

  private async Task<(string DraftPartPublicId, string SetPublicId)> CreateDraftPartWithSetAsync()
  {
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);
    var setPublicId = await SubmitSetAsync(
      draftPartPublicId, seasonPublicId, contestantPublicId,
      "m_00000001", "m_00000002", "m_00000003");
    return (draftPartPublicId, setPublicId);
  }
}

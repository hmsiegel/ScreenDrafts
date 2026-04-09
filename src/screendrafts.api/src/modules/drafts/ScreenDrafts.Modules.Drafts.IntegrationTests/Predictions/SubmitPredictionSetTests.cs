namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class SubmitPredictionSetTests(DraftsIntegrationTestWebAppFactory factory)
  : PredictionIntegrationTestBase(factory)
{
  [Fact]
  public async Task SubmitPredictionSet_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var entries = MakeEntries("m_00000001", "m_00000002", "m_00000003");
    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = entries
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SubmitPredictionSet_ShouldPersistEntries_ToDatabaseAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002", "m_00000003")
    };

    // Act
    await Sender.Send(command);

    // Assert
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.PublicId == draftPartPublicId);
    var contestant = await DbContext.PredictionContestants.FirstAsync(c => c.PublicId == contestantPublicId);
    var set = await DbContext.DraftPredictionSets
      .Include(s => s.Entries)
      .FirstOrDefaultAsync(s => s.DraftPartId == draftPart.Id && s.ContestantId == contestant.Id);

    set.Should().NotBeNull();
    set!.Entries.Should().HaveCount(3);
  }

  [Fact]
  public async Task SubmitPredictionSet_WhenDuplicateForSameContestantAndDraftPart_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002", "m_00000003")
    };

    await Sender.Send(command);

    // Act — submit again for same contestant and draft part
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.SetAlreadyExists);
  }

  [Fact]
  public async Task SubmitPredictionSet_WithWrongEntryCount_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002") // only 2, but 3 required
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.InvalidEntryCount");
  }

  [Fact]
  public async Task SubmitPredictionSet_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = "dp_nonexistent123",
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002", "m_00000003")
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task SubmitPredictionSet_WithNonExistentSeason_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = "ps_nonexistent123",
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002", "m_00000003")
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task SubmitPredictionSet_WithNonExistentContestant_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    await SetRulesAsync(draftPartPublicId, requiredCount: 3);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = "pc_nonexistent123",
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002", "m_00000003")
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task SubmitPredictionSet_WhenNoRulesExist_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();
    // Intentionally NOT setting rules

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002", "m_00000003")
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.RulesNotFound");
  }

  [Fact]
  public async Task SubmitPredictionSet_AfterDeadlinePassed_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var seasonPublicId = await CreateSeasonPublicIdAsync();
    var contestantPublicId = await CreateContestantPublicIdAsync();

    // Set rules with a past deadline
    await Sender.Send(new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = PredictionMode.UnorderedAll.Value,
      RequiredCount = 3,
      DeadlineUtc = DateTime.UtcNow.AddDays(-1)
    });

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries("m_00000001", "m_00000002", "m_00000003")
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.DeadlinePassed);
  }

  // ──────────────────────────────────────────────────────────────────────
  // Helpers
  // ──────────────────────────────────────────────────────────────────────

  private static IReadOnlyList<PredictionEntryDto> MakeEntries(params string[] mediaIds) =>
    [.. mediaIds.Select((id, i) => new PredictionEntryDto { MediaPublicId = id, MediaTitle = $"Movie {i + 1}" })];
}

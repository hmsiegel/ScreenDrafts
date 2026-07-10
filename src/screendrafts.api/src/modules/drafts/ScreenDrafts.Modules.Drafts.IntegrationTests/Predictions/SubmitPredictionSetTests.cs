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
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);

    var entries = MakeEntries(1, 2, 3);
    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = entries,
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries(1, 2, 3),
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);
    var contestant = await DbContext.PredictionContestants.FirstAsync(c => c.PublicId == contestantPublicId, TestContext.Current.CancellationToken);
    var set = await DbContext.DraftPredictionSets
      .Include(s => s.Entries)
      .FirstOrDefaultAsync(s => s.DraftPartId == draftPart.Id && s.ContestantId == contestant.Id, TestContext.Current.CancellationToken);

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
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries(1, 2, 3),
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — submit again for same contestant and draft part
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries(1, 2), // only 2, but 3 required
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
      Entries = MakeEntries(1, 2, 3),
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = "ps_nonexistent123",
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries(1, 2, 3),
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
      Entries = MakeEntries(1, 2, 3),
      ActorUserPublicId = $"u_{Faker.Random.AlphaNumeric(16)}"
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);
    // Intentionally NOT setting rules

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries(1, 2, 3),
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
    }, TestContext.Current.CancellationToken);
    await EnsurePredictorConfiguredAsync(draftPartPublicId, contestantPublicId);

    var command = new SubmitPredictionSetCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SeasonPublicId = seasonPublicId,
      ContestantPublicId = contestantPublicId,
      SourceKind = PredictionSourceKind.UI.Value,
      Entries = MakeEntries(1, 2, 3),
      ActorUserPublicId = GetActorUserPublicIdForContestant(contestantPublicId)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.DeadlinePassed);
  }

  // ──────────────────────────────────────────────────────────────────────
  // Helpers
  // ──────────────────────────────────────────────────────────────────────

  private static IReadOnlyList<PredictionEntryDto> MakeEntries(params int[] tmdbIds) =>
    [.. tmdbIds.Select((id, i) => new PredictionEntryDto { TmdbId = id, MediaTitle = $"Movie {i + 1}" })];
}

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Predictions;

public sealed class SetDraftPartPredictionRulesTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetRules_WithUnorderedAll_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = PredictionMode.UnorderedAll.Value,
      RequiredCount = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetRules_WithUnorderedTopN_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = PredictionMode.UnorderedTopN.Value,
      RequiredCount = 7,
      TopN = 10
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetRules_ShouldPersistToDatabase_WhenSuccessfulAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = PredictionMode.UnorderedAll.Value,
      RequiredCount = 5
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);
    var rules = await DbContext.DraftPartPredictionRules
      .FirstOrDefaultAsync(r => r.DraftPartId == draftPart.Id, TestContext.Current.CancellationToken);
    rules.Should().NotBeNull();
    rules!.RequiredCount.Should().Be(5);
  }

  [Fact]
  public async Task SetRules_WhenRulesAlreadyExist_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = PredictionMode.UnorderedAll.Value,
      RequiredCount = 7
    };

    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — attempt to set rules again
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.RulesAlreadyExist");
  }

  [Fact]
  public async Task SetRules_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = "dp_nonexistent123",
      PredictionMode = PredictionMode.UnorderedAll.Value,
      RequiredCount = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task SetRules_WithTopNModeButNoTopN_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = PredictionMode.UnorderedTopN.Value,
      RequiredCount = 7
      // TopN intentionally omitted
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.TopNRequiredForPredictionMode");
  }

  [Fact]
  public async Task SetRules_WithAllModeAndTopN_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetDraftPartPredictionRulesCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PredictionMode = PredictionMode.UnorderedAll.Value,
      RequiredCount = 7,
      TopN = 5  // should not be set for All modes
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().ContainSingle(e =>
      e.Code == "PredictionErrors.TopNNotAllowedForPredictionMode");
  }

  // ──────────────────────────────────────────────────────────────────────
  // Helpers
  // ──────────────────────────────────────────────────────────────────────

  private async Task<string> CreateDraftPartPublicIdAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftPublicIdAsync(seriesId);
    var result = await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<string> CreateDraftPublicIdAsync(Guid seriesId)
  {
    var result = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }
}

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class CreateDraftPartTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateDraftPart_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithoutPartAsync();
    var command = new CreateDraftPartCommand
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task CreateDraftPart_ShouldReturnPublicId_ThatMatchesDraftPartInDatabaseAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithoutPartAsync();
    var command = new CreateDraftPartCommand
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var exists = await DbContext.DraftParts
      .AnyAsync(dp => dp.PublicId == result.Value, TestContext.Current.CancellationToken);
    exists.Should().BeTrue();
  }

  [Fact]
  public async Task CreateDraftPart_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateDraftPartCommand
    {
      DraftPublicId = Faker.Random.AlphaNumeric(10),
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateDraftPart_WithEmptyDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateDraftPartCommand
    {
      DraftPublicId = string.Empty,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateDraftPart_WithZeroPartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithoutPartAsync();
    var command = new CreateDraftPartCommand
    {
      DraftPublicId = draftId,
      PartIndex = 0,
      MinimumPosition = 1,
      MaximumPosition = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateDraftPart_WithNegativePartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithoutPartAsync();
    var command = new CreateDraftPartCommand
    {
      DraftPublicId = draftId,
      PartIndex = -1,
      MinimumPosition = 1,
      MaximumPosition = 7
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateDraftPart_WithDuplicatePartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithoutPartAsync();
    var command = new CreateDraftPartCommand
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    };

    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Act — same PartIndex again
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateDraftPart_MultipleParts_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithoutPartAsync();

    // Act
    var result1 = await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      MinimumPosition = 15,
      MaximumPosition = 21
    }, TestContext.Current.CancellationToken);
    var result2 = await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftId,
      PartIndex = 2,
      MinimumPosition = 8,
      MaximumPosition = 14
    }, TestContext.Current.CancellationToken);

    // Assert
    result1.IsSuccess.Should().BeTrue();
    result2.IsSuccess.Should().BeTrue();
    result1.Value.Should().NotBe(result2.Value);
  }

  // -------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------

  private async Task<string> CreateDraftWithoutPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    };

    var result = await Sender.Send(command, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var command = new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    };

    var result = await Sender.Send(command, TestContext.Current.CancellationToken);
    return result.Value;
  }
}

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class AddSubDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddSubDraft_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupSpeedDraftPartAsync();

    var command = new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task AddSubDraft_ShouldPersistSubDraftInDatabaseAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupSpeedDraftPartAsync();

    var command = new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    var subDraftPublicId = result.Value;
    var draftPart = await DbContext.DraftParts
      .Include("_subDrafts")
      .FirstAsync(dp => dp.PublicId == draftPartPublicId);

    draftPart.SubDrafts.Should().ContainSingle();
    draftPart.SubDrafts.First().PublicId.Should().Be(subDraftPublicId);
    draftPart.SubDrafts.First().Index.Should().Be(0);
    draftPart.SubDrafts.First().Status.Should().Be(SubDraftStatus.Pending);
  }

  [Fact]
  public async Task AddSubDraft_WithMultipleIndices_ShouldPersistAllAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupSpeedDraftPartAsync();

    // Act
    await Sender.Send(new AddSubDraftCommand { DraftPartPublicId = draftPartPublicId, Index = 0 });
    await Sender.Send(new AddSubDraftCommand { DraftPartPublicId = draftPartPublicId, Index = 1 });
    var result = await Sender.Send(new AddSubDraftCommand { DraftPartPublicId = draftPartPublicId, Index = 2 });

    // Assert
    result.IsSuccess.Should().BeTrue();

    var draftPart = await DbContext.DraftParts
      .Include("_subDrafts")
      .AsNoTracking()
      .FirstAsync(dp => dp.PublicId == draftPartPublicId);

    draftPart.SubDrafts.Should().HaveCount(3);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddSubDraft_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new AddSubDraftCommand
    {
      DraftPartPublicId = Faker.Random.AlphaNumeric(10),
      Index = 0
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — non-SpeedDraft part
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddSubDraft_OnStandardDraftPart_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupStandardDraftPartAsync();

    var command = new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — duplicate index
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddSubDraft_WithDuplicateIndex_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupSpeedDraftPartAsync();
    await Sender.Send(new AddSubDraftCommand { DraftPartPublicId = draftPartPublicId, Index = 0 });

    var command = new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<string> SetupSpeedDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    return await GetFirstDraftPartPublicIdAsync(draftPublicId);
  }

  private async Task<string> SetupStandardDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateStandardDraftAsync(seriesId);
    return await GetFirstDraftPartPublicIdAsync(draftPublicId);
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
      DefaultDraftType = DraftType.SpeedDraft.Value
    });
    return result.Value;
  }

  private async Task<string> CreateSpeedDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.SpeedDraft.Value,
      SeriesId = seriesId,
    });
    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    });
    return draftPublicId;
  }

  private async Task<string> CreateStandardDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    });
    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    });
    return draftPublicId;
  }
}

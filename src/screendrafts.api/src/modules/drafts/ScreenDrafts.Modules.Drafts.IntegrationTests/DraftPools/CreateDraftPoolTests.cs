namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftPools;

public sealed class CreateDraftPoolTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task CreateDraftPool_WithValidDraftId_ShouldSucceedAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();

    // Act
    var result = await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task CreateDraftPool_ShouldPersistPoolInDatabaseAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();

    // Act
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });

    // Assert
    var pool = await DbContext.DraftPools.FirstOrDefaultAsync();
    pool.Should().NotBeNull();
    pool!.PublicId.Should().NotBeNullOrEmpty();
    pool.IsLocked.Should().BeFalse();
  }

  [Fact]
  public async Task CreateDraftPool_ShouldLinkPoolToDraftAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();

    // Act
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });

    // Assert
    var draft = await DbContext.Drafts.FirstAsync(d => d.PublicId == draftPublicId);
    var pool = await DbContext.DraftPools.FirstOrDefaultAsync(p => p.DraftId == draft.Id);
    pool.Should().NotBeNull();
  }

  // ---------------------------------------------------------------------------
  // Guard — draft not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task CreateDraftPool_WithNonExistentDraft_ShouldFailAsync()
  {
    // Act
    var result = await Sender.Send(new CreateDraftPoolCommand { PublicId = "nonexistent-draft" });

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — pool already exists
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task CreateDraftPool_WhenPoolAlreadyExists_ShouldFailAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });

    // Act — create pool again
    var result = await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private async Task<string> CreateDraftAsync()
  {
    var seriesResult = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });

    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesResult.Value,
    });

    return draftResult.Value;
  }
}

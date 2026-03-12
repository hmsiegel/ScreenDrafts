namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftPools;

public sealed class GetDraftPoolTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task GetDraftPool_ShouldReturnPool_WhenPoolExistsAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();

    // Act
    var result = await Sender.Send(new GetDraftPoolQuery { PublicId = draftPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DraftId.Should().Be(draftPublicId);
    result.Value.IsLocked.Should().BeFalse();
  }

  [Fact]
  public async Task GetDraftPool_ShouldReturnEmptyTmdbIds_WhenNoMoviesAddedAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();

    // Act
    var result = await Sender.Send(new GetDraftPoolQuery { PublicId = draftPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TmdbIds.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraftPool_ShouldReturnTmdbIds_WhenMoviesAddedAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);
    await CreateMovieInDbAsync(tmdbId1);
    await CreateMovieInDbAsync(tmdbId2);
    await Sender.Send(new AddMovieToDraftPoolCommand { PublicId = draftPublicId, TmdbId = tmdbId1 });
    await Sender.Send(new AddMovieToDraftPoolCommand { PublicId = draftPublicId, TmdbId = tmdbId2 });

    // Act
    var result = await Sender.Send(new GetDraftPoolQuery { PublicId = draftPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TmdbIds.Should().HaveCount(2);
    result.Value.TmdbIds.Should().Contain(tmdbId1);
    result.Value.TmdbIds.Should().Contain(tmdbId2);
  }

  [Fact]
  public async Task GetDraftPool_ShouldReturnPublicId_WhenPoolExistsAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();

    // Act
    var result = await Sender.Send(new GetDraftPoolQuery { PublicId = draftPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().NotBeNullOrEmpty();
  }

  // ---------------------------------------------------------------------------
  // Guard — pool not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task GetDraftPool_ShouldFail_WhenPoolNotFoundAsync()
  {
    // Act
    var result = await Sender.Send(new GetDraftPoolQuery { PublicId = "nonexistent-draft" });

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task GetDraftPool_ShouldFail_WhenDraftExistsButHasNoPoolAsync()
  {
    // Arrange — draft without pool
    var draftPublicId = await CreateDraftAsync();

    // Act
    var result = await Sender.Send(new GetDraftPoolQuery { PublicId = draftPublicId });

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

  private async Task<string> CreateDraftWithPoolAsync()
  {
    var draftPublicId = await CreateDraftAsync();
    await Sender.Send(new CreateDraftPoolCommand { PublicId = draftPublicId });
    return draftPublicId;
  }
}

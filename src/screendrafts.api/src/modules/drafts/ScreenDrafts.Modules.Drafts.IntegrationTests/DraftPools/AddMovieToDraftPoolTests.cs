namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftPools;

public sealed class AddMovieToDraftPoolTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ---------------------------------------------------------------------------
  // Happy path
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    // Act
    var result = await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = draftPublicId,
      TmdbId = tmdbId
    });

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddMovie_ShouldPersistMovieInPoolAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    // Act
    await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = draftPublicId,
      TmdbId = tmdbId
    });

    // Assert
    var pool = await DbContext.DraftPools
      .Include(p => p.TmdbIds)
      .FirstAsync();
    pool.TmdbIds.Should().Contain(i => i.TmdbId == tmdbId);
  }

  [Fact]
  public async Task AddMovie_ShouldSucceed_WhenMovieNotInMovieDb_AndDispatchFetchEventAsync()
  {
    // Arrange — movie not pre-seeded in drafts.movies table
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId = Faker.Random.Int(1_000_000, 9_000_000);

    // Act — should succeed even without movie in DB; a fetch event is published
    var result = await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = draftPublicId,
      TmdbId = tmdbId
    });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pool = await DbContext.DraftPools
      .Include(p => p.TmdbIds)
      .FirstAsync();
    pool.TmdbIds.Should().Contain(i => i.TmdbId == tmdbId);
  }

  // ---------------------------------------------------------------------------
  // Guard — draft not found
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WithNonExistentDraft_ShouldFailAsync()
  {
    // Act
    var result = await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = "nonexistent",
      TmdbId = 1
    });

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — pool not found for draft
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WhenPoolDoesNotExist_ShouldFailAsync()
  {
    // Arrange — draft created but no pool
    var draftPublicId = await CreateDraftAsync();

    // Act
    var result = await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = draftPublicId,
      TmdbId = 1
    });

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ---------------------------------------------------------------------------
  // Guard — duplicate movie
  // ---------------------------------------------------------------------------

  [Fact]
  public async Task AddMovie_WhenMovieAlreadyInPool_ShouldFailAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithPoolAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);
    await Sender.Send(new AddMovieToDraftPoolCommand { PublicId = draftPublicId, TmdbId = tmdbId });

    // Act — add same movie again
    var result = await Sender.Send(new AddMovieToDraftPoolCommand
    {
      PublicId = draftPublicId,
      TmdbId = tmdbId
    });

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

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.CandidateLists;

public sealed class AddCandidateListEntryTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddCandidateListEntry_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var command = new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TmdbId.Should().Be(tmdbId);
    result.Value.EntryId.Should().NotBeEmpty();
  }

  [Fact]
  public async Task AddCandidateListEntry_ShouldPersistEntryInDatabaseAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var command = new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    await Sender.Send(command);

    // Assert
    var entry = await DbContext.CandidateListEntries
      .FirstOrDefaultAsync(e => e.TmdbId == tmdbId);

    entry.Should().NotBeNull();
    entry!.TmdbId.Should().Be(tmdbId);
    entry.DraftPartPublicId.Should().Be(draftPartPublicId);
  }

  [Fact]
  public async Task AddCandidateListEntry_ShouldBePending_WhenMovieNotInDbAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var command = new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Value.IsPending.Should().BeTrue();
  }

  [Fact]
  public async Task AddCandidateListEntry_ShouldNotBePending_WhenMovieAlreadyInDbAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    var command = new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Value.IsPending.Should().BeFalse();
  }

  [Fact]
  public async Task AddCandidateListEntry_ShouldPersistNotes_WhenProvided()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    var notes = Faker.Lorem.Sentence();

    var command = new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      Notes = notes,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    await Sender.Send(command);

    // Assert
    var entry = await DbContext.CandidateListEntries
      .FirstOrDefaultAsync(e => e.TmdbId == tmdbId);

    entry!.Notes.Should().Be(notes);
  }

  // -------------------------------------------------------------------------
  // Guard — idempotent re-add
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddCandidateListEntry_ShouldReturnExistingEntry_WhenTmdbIdAlreadyAddedAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    var addedByPublicId = "u_" + Faker.Random.AlphaNumeric(17);

    var command = new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = addedByPublicId
    };

    var first = await Sender.Send(command);

    // Act — send the same command again
    var second = await Sender.Send(command);

    // Assert
    second.IsSuccess.Should().BeTrue();
    second.Value.EntryId.Should().Be(first.Value.EntryId);
    second.Value.TmdbId.Should().Be(tmdbId);

    var count = await DbContext.CandidateListEntries
      .CountAsync(e => e.TmdbId == tmdbId);
    count.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddCandidateListEntry_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Arrange
    var command = new AddCandidateEntryCommand
    {
      DraftPartId = "dp_nonexistent",
      TmdbId = Faker.Random.Int(1, 1_000_000),
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Code.Should().Be(CandidateListErrors.DraftPartNotFound("dp_nonexistent").Code);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<string> SetupDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAndPartAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));

    return draftPart.PublicId;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });

    return result.Value;
  }

  private async Task<string> CreateDraftAndPartAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    });

    var draftPublicId = draftResult.Value;

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    });

    return draftPublicId;
  }
}

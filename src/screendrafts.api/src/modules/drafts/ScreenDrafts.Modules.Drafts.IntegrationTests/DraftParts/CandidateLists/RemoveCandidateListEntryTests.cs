namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.CandidateLists;

public sealed class RemoveCandidateListEntryTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RemoveCandidateListEntry_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    }, TestContext.Current.CancellationToken);

    var command = new RemoveCandidateListEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveCandidateListEntry_ShouldDeleteFromDatabaseAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    }, TestContext.Current.CancellationToken);

    var command = new RemoveCandidateListEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var entry = await DbContext.CandidateListEntries
      .FirstOrDefaultAsync(e => e.TmdbId == tmdbId, TestContext.Current.CancellationToken);

    entry.Should().BeNull();
  }

  [Fact]
  public async Task RemoveCandidateListEntry_ShouldOnlyRemoveTargetEntry_WhenMultipleEntriesExistAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbIdToRemove = Faker.Random.Int(1, 500_000);
    var tmdbIdToKeep = Faker.Random.Int(500_001, 1_000_000);
    var addedByPublicId = "u_" + Faker.Random.AlphaNumeric(17);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbIdToRemove,
      AddedByPublicId = addedByPublicId
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbIdToKeep,
      AddedByPublicId = addedByPublicId
    }, TestContext.Current.CancellationToken);

    // Act
    await Sender.Send(new RemoveCandidateListEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbIdToRemove
    }, TestContext.Current.CancellationToken);

    // Assert
    var remaining = await DbContext.CandidateListEntries.ToListAsync(TestContext.Current.CancellationToken);
    remaining.Should().ContainSingle(e => e.TmdbId == tmdbIdToKeep);
    remaining.Should().NotContain(e => e.TmdbId == tmdbIdToRemove);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RemoveCandidateListEntry_ShouldFail_WhenDraftPartNotFoundAsync()
  {
    // Arrange
    var command = new RemoveCandidateListEntryCommand
    {
      DraftPartId = "dp_nonexistent",
      TmdbId = Faker.Random.Int(1, 1_000_000)
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Code.Should().Be(CandidateListErrors.DraftPartNotFound("dp_nonexistent").Code);
  }

  // -------------------------------------------------------------------------
  // Guard — entry not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RemoveCandidateListEntry_ShouldFail_WhenEntryNotFoundAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var nonExistentTmdbId = Faker.Random.Int(1, 1_000_000);

    var command = new RemoveCandidateListEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = nonExistentTmdbId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Code.Should().Be(CandidateListErrors.EntryNotFound(nonExistentTmdbId).Code);
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
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);

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
    }, TestContext.Current.CancellationToken);

    return result.Value;
  }

  private async Task<string> CreateDraftAndPartAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    }, TestContext.Current.CancellationToken);

    var draftPublicId = draftResult.Value;

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    }, TestContext.Current.CancellationToken);

    return draftPublicId;
  }
}

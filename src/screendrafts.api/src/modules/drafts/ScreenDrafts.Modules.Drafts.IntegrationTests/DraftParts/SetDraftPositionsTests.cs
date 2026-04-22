namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class SetDraftPositionsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SetDraftPositions_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, draftPartInternalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(draftPartInternalId);
    await AddDrafterToPartAsync(draftPartInternalId);

    var command = new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "Position 1", Picks = [1, 3] },
        new DraftPositionRequest { Name = "Position 2", Picks = [2, 4] }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetDraftPositions_ShouldPersistPositionsInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, draftPartInternalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(draftPartInternalId);
    await AddDrafterToPartAsync(draftPartInternalId);

    var command = new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "Alpha", Picks = [1] },
        new DraftPositionRequest { Name = "Beta", Picks = [2] }
      ]
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var positionCount = await DbContext.DraftPositions
      .CountAsync(p => p.GameBoard.DraftPartId == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);

    positionCount.Should().Be(2);
  }

  [Fact]
  public async Task SetDraftPositions_ShouldReplaceExistingPositionsAsync()
  {
    // Arrange — set positions once with original names
    var (draftPartPublicId, draftPartInternalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(draftPartInternalId);
    await AddDrafterToPartAsync(draftPartInternalId);

    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "Old 1", Picks = [1] },
        new DraftPositionRequest { Name = "Old 2", Picks = [2] }
      ]
    }, TestContext.Current.CancellationToken);

    // Act — set positions again with new names
    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "New 1", Picks = [1] },
        new DraftPositionRequest { Name = "New 2", Picks = [2] }
      ]
    }, TestContext.Current.CancellationToken);

    // Assert — only 2 positions remain (replaced, not accumulated)
    var positions = await DbContext.DraftPositions
      .Where(p => p.GameBoard.DraftPartId == DraftPartId.Create(draftPartInternalId))
      .ToListAsync(TestContext.Current.CancellationToken);

    positions.Should().HaveCount(2);
    positions.Select(p => p.Name).Should().BeEquivalentTo("New 1", "New 2");
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SetDraftPositions_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new SetDraftPositionsCommand
    {
      DraftPartId = Faker.Random.AlphaNumeric(10),
      Positions =
      [
        new DraftPositionRequest { Name = "Position 1", Picks = [1] }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — position count mismatch
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SetDraftPositions_WhenPositionCountMismatch_ShouldFailAsync()
  {
    // Arrange — 2 participants but only 1 position
    var (draftPartPublicId, draftPartInternalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(draftPartInternalId);
    await AddDrafterToPartAsync(draftPartInternalId);

    var command = new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "Position 1", Picks = [1] }
      ]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string PublicId, Guid InternalId)> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var internalId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(internalId), TestContext.Current.CancellationToken);

    var gameBoard = GameBoard.Create(draftPart).Value;
    DbContext.GameBoards.Add(gameBoard);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    return (draftPart.PublicId, internalId);
  }

  private async Task<string> AddDrafterToPartAsync(Guid draftPartInternalId)
  {
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);
    var draftPartPublicId = draftPart.PublicId;

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personId), TestContext.Current.CancellationToken)).Value;

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    return drafterPublicId;
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

  private async Task<string> CreateDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    }, TestContext.Current.CancellationToken);

    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    }, TestContext.Current.CancellationToken);

    return draftPublicId;
  }
}

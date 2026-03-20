namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class ClearDraftPositionAssignmentTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ClearAssignment_WithAssignedPosition_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, positionPublicId, _) = await SetupDraftPartWithAssignedPositionAsync();

    var command = new ClearDraftPositionAssignmentCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ClearAssignment_ShouldClearAssignmentInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, positionPublicId, _) = await SetupDraftPartWithAssignedPositionAsync();

    // Act
    await Sender.Send(new ClearDraftPositionAssignmentCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId
    });

    // Assert
    var position = await DbContext.DraftPositions
      .FirstAsync(p => p.PublicId == positionPublicId);

    position.AssignedToId.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ClearAssignment_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new ClearDraftPositionAssignmentCommand
    {
      DraftPartId = Faker.Random.AlphaNumeric(10),
      PositionPublicId = Faker.Random.AlphaNumeric(10)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — position not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ClearAssignment_WithNonExistentPosition_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _, _) = await SetupDraftPartWithAssignedPositionAsync();

    var command = new ClearDraftPositionAssignmentCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = Faker.Random.AlphaNumeric(10)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — position not assigned
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ClearAssignment_WhenPositionNotAssigned_ShouldFailAsync()
  {
    // Arrange — create a position without assigning it
    var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(internalId);

    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions = [new DraftPositionRequest { Name = "Slot 1", Picks = [1] }]
    });

    var positionPublicId = await DbContext.DraftPositions
      .Where(p => p.GameBoard.DraftPartId == DraftPartId.Create(internalId))
      .Select(p => p.PublicId)
      .FirstAsync();

    var command = new ClearDraftPositionAssignmentCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string PositionPublicId, Guid DraftPartInternalId)>
    SetupDraftPartWithAssignedPositionAsync()
  {
    var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
    var drafterInternalId = await AddDrafterToPartAsync(internalId);

    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions = [new DraftPositionRequest { Name = "Slot 1", Picks = [1] }]
    });

    var position = await DbContext.DraftPositions
      .Include(p => p.GameBoard)
      .ThenInclude(gb => gb.DraftPart)
      .FirstAsync(p => p.GameBoard.DraftPartId == DraftPartId.Create(internalId));

    position.AssignParticipant(new Participant(drafterInternalId, ParticipantKind.Drafter));
    await DbContext.SaveChangesAsync();

    return (draftPartPublicId, position.PublicId, internalId);
  }

  private async Task<(string PublicId, Guid InternalId)> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var internalId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(internalId));

    var gameBoard = GameBoard.Create(draftPart).Value;
    DbContext.GameBoards.Add(gameBoard);
    await DbContext.SaveChangesAsync();

    return (draftPart.PublicId, internalId);
  }

  private async Task<Guid> AddDrafterToPartAsync(Guid draftPartInternalId)
  {
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personId))).Value;

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPart.PublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    return await DbContext.Drafters
      .Where(d => d.PublicId == drafterPublicId)
      .Select(d => d.Id.Value)
      .FirstAsync();
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
    });

    return result.Value;
  }

  private async Task<string> CreateDraftAsync(Guid seriesId)
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

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class AssignParticipantToDraftPositionTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignParticipant_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, positionPublicId, _, _) = await SetupDraftPartWithPositionAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var newDrafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId,
      ParticipantPublicId = newDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AssignParticipant_ShouldPersistParticipantInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, positionPublicId, draftPartInternalId, _) = await SetupDraftPartWithPositionAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var newDrafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId,
      ParticipantPublicId = newDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert — the new drafter is now in the draft part participant list
    var participantExists = await DbContext.DraftPartParticipants
      .AnyAsync(p => p.DraftPartId == DraftPartId.Create(draftPartInternalId)
                  && p.ParticipantKindValue == ParticipantKind.Drafter, TestContext.Current.CancellationToken);

    participantExists.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignParticipant_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = Faker.Random.AlphaNumeric(10),
      PositionPublicId = Faker.Random.AlphaNumeric(10),
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — position not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignParticipant_WithNonExistentPosition_ShouldFailAsync()
  {
    // Arrange — valid draft part with positions, but using a different position ID
    var (draftPartPublicId, _, _, _) = await SetupDraftPartWithPositionAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var newDrafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = Faker.Random.AlphaNumeric(10),
      ParticipantPublicId = newDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — participant not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignParticipant_WithNonExistentParticipant_ShouldFailAsync()
  {
    // Arrange — valid draft part with a position, but non-existent participant
    var (draftPartPublicId, positionPublicId, _, _) = await SetupDraftPartWithPositionAsync();

    var command = new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId,
      ParticipantPublicId = Faker.Random.AlphaNumeric(10),
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Participant already in roster — assigning to a position should succeed
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AssignParticipant_WhenParticipantAlreadyInRoster_ShouldSucceedAsync()
  {
    // Arrange — the initial drafter is already in the roster (added via AddParticipant in setup).
    // Assigning them to a position is valid: the roster-add is skipped and only the
    // position assignment happens.
    var (draftPartPublicId, positionPublicId, _, existingDrafterPublicId) = await SetupDraftPartWithPositionAsync();

    var command = new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = draftPartPublicId,
      PositionPublicId = positionPublicId,
      ParticipantPublicId = existingDrafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string PositionPublicId, Guid DraftPartInternalId, string DrafterPublicId)>
    SetupDraftPartWithPositionAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);
    var draftPartPublicId = draftPart.PublicId;

    var gameBoard = GameBoard.Create(draftPart).Value;
    DbContext.GameBoards.Add(gameBoard);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Add one participant so we can set exactly one position
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personId), TestContext.Current.CancellationToken)).Value;

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    // Set one position (count must equal participant count = 1)
    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "Slot 1", Picks = [1] }
      ]
    }, TestContext.Current.CancellationToken);

    var positionPublicId = await DbContext.DraftPositions
      .Where(p => p.GameBoard.DraftPartId == DraftPartId.Create(draftPartInternalId))
      .Select(p => p.PublicId)
      .FirstAsync(TestContext.Current.CancellationToken);

    return (draftPartPublicId, positionPublicId, draftPartInternalId, drafterPublicId);
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

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class ListDraftPositionsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path — no positions
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListDraftPositions_WhenNoPositionsExist_ShouldReturnEmptyListAsync()
  {
    // Arrange — draft part with a game board but no positions set
    var (draftPartPublicId, _) = await CreateDraftPartAsync();

    var query = new ListDraftPositionsQuery { DraftPartId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Positions.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Happy path — positions returned
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListDraftPositions_WhenPositionsExist_ShouldReturnThemAsync()
  {
    // Arrange
    var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(internalId);
    await AddDrafterToPartAsync(internalId);

    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions =
      [
        new DraftPositionRequest { Name = "Alpha", Picks = [1, 3] },
        new DraftPositionRequest { Name = "Beta", Picks = [2, 4] }
      ]
    }, TestContext.Current.CancellationToken);

    var query = new ListDraftPositionsQuery { DraftPartId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Positions.Should().HaveCount(2);
    result.Value.Positions.Select(p => p.Name).Should().BeEquivalentTo("Alpha", "Beta");
  }

  [Fact]
  public async Task ListDraftPositions_ShouldReturnPublicIdsAsync()
  {
    // Arrange
    var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(internalId);

    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions = [new DraftPositionRequest { Name = "Slot 1", Picks = [1] }]
    }, TestContext.Current.CancellationToken);

    var expectedPublicId = await DbContext.DraftPositions
      .Where(p => p.GameBoard.DraftPartId == DraftPartId.Create(internalId))
      .Select(p => p.PublicId)
      .FirstAsync(TestContext.Current.CancellationToken);

    var query = new ListDraftPositionsQuery { DraftPartId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Positions.Should().ContainSingle()
      .Which.PublicId.Should().Be(expectedPublicId);
  }

  // -------------------------------------------------------------------------
  // Happy path — assignment populated
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListDraftPositions_WhenPositionIsAssigned_ShouldReturnAssignmentInfoAsync()
  {
    // Arrange
    var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
    var drafterInternalId = await AddDrafterToPartAsync(internalId);

    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions = [new DraftPositionRequest { Name = "Slot 1", Picks = [1] }]
    }, TestContext.Current.CancellationToken);

    var position = await DbContext.DraftPositions
      .Include(p => p.GameBoard)
      .ThenInclude(gb => gb.DraftPart)
      .FirstAsync(p => p.GameBoard.DraftPartId == DraftPartId.Create(internalId), TestContext.Current.CancellationToken);

    position.AssignParticipant(new Participant(drafterInternalId, ParticipantKind.Drafter));
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    var query = new ListDraftPositionsQuery { DraftPartId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var assignedPosition = result.Value.Positions.Should().ContainSingle().Subject;
    assignedPosition.AssignedTo.Should().NotBeNull();
    assignedPosition.AssignedTo!.ParticipantId.Should().Be(drafterInternalId);
    assignedPosition.AssignedTo.ParticipantKind.Should().Be(ParticipantKind.Drafter.Value);
  }

  [Fact]
  public async Task ListDraftPositions_WhenPositionIsNotAssigned_ShouldHaveNullAssignmentAsync()
  {
    // Arrange
    var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
    await AddDrafterToPartAsync(internalId);

    await Sender.Send(new SetDraftPositionsCommand
    {
      DraftPartId = draftPartPublicId,
      Positions = [new DraftPositionRequest { Name = "Slot 1", Picks = [1] }]
    }, TestContext.Current.CancellationToken);

    var query = new ListDraftPositionsQuery { DraftPartId = draftPartPublicId };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Positions.Should().ContainSingle()
      .Which.AssignedTo.Should().BeNull();
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

  private async Task<Guid> AddDrafterToPartAsync(Guid draftPartInternalId)
  {
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personId), TestContext.Current.CancellationToken)).Value;

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPart.PublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    return await DbContext.Drafters
      .Where(d => d.PublicId == drafterPublicId)
      .Select(d => d.Id.Value)
      .FirstAsync(TestContext.Current.CancellationToken);
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

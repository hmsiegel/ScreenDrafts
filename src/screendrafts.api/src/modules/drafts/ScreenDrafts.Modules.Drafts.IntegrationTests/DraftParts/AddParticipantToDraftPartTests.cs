namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class AddParticipantToDraftPartTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddParticipant_WithValidDrafter_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartInternalId = await CreateDraftPartAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddParticipant_ShouldPersistParticipantInDatabaseAsync()
  {
    // Arrange
    var draftPartInternalId = await CreateDraftPartAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    await Sender.Send(command);

    // Assert
    var participantPublicId = await GetFirstParticipantPublicIdAsync(draftPartInternalId);
    participantPublicId.Should().Be(drafterPublicId);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddParticipant_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AddParticipantToDraftPartCommand
    {
      DraftPartId = Guid.NewGuid(),
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — participant not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddParticipant_WithNonExistentDrafter_ShouldFailAsync()
  {
    // Arrange
    var draftPartInternalId = await CreateDraftPartAsync();

    var command = new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      ParticipantPublicId = Faker.Random.AlphaNumeric(10),
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — duplicate participant
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddParticipant_WhenParticipantAlreadyAdded_ShouldFailAsync()
  {
    // Arrange
    var draftPartInternalId = await CreateDraftPartAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Add once — should succeed
    await Sender.Send(command);

    // Act — add same participant again
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<Guid> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    return await GetFirstDraftPartIdAsync(draftPublicId);
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
    var result = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
      MinPosition = 1,
      MaxPosition = 7,
      AutoCreateFirstPart = true
    });

    return result.Value;
  }
}

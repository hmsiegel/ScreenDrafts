namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SetDraftPartStatusTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetDraftPartStatus_StartAction_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPublicId, draftPartId) = await CreateDraftWithAutoPartAndSetupAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
  }

  [Fact]
  public async Task SetDraftPartStatus_CompleteAction_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPublicId, draftPartId) = await CreateDraftWithAutoPartAndSetupAsync();

    // Start the part first
    var startRequest = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    };
    await Sender.Send(new SetDraftPartStatusCommand { SetDraftPartStatusRequest = startRequest });

    // Add all required picks
    await AddRequiredPicksToDraftPartAsync(draftPartId);

    // Complete the part
    var completeRequest = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Complete
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = completeRequest
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = Faker.Random.AlphaNumeric(10),
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithInvalidPartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 999,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithZeroPartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 0,
      Action = DraftPartStatusAction.Start
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_CompleteWithoutStart_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var request = new SetDraftPartStatusRequest
    {
      DraftPublicId = draftId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Complete
    };
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = request
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  private async Task<string> CreateDraftWithAutoPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
      MinPosition = 1,
      MaxPosition = 7,
      AutoCreateFirstPart = true
    };

    var result = await Sender.Send(command);
    return result.Value;
  }

  private async Task<(string draftPublicId, Guid draftPartId)> CreateDraftWithAutoPartAndSetupAsync()
  {
    var draftPublicId = await CreateDraftWithAutoPartAsync();

    // Get the draft part ID
    var draft = await DbContext.Drafts
      .Include(d => d.Parts)
      .FirstOrDefaultAsync(d => d.PublicId == draftPublicId);

    var draftPartId = draft!.Parts.First().Id.Value;

    // Create and add participants (need at least 2)
    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1Result = await Sender.Send(new CreateDrafterCommand(person1Id));
    var drafter1Id = Guid.Parse(drafter1Result.Value);
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartId,
      ParticipantId = drafter1Id,
      ParticipantKind = ParticipantKind.Drafter
    };

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2Result = await Sender.Send(new CreateDrafterCommand(person2Id));
    var drafter2Id = Guid.Parse(drafter2Result.Value);
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartId,
      ParticipantId = drafter2Id,
      ParticipantKind = ParticipantKind.Drafter
    });

    // Create and add a host
    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostResult = await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId });
    var hostId = Guid.Parse(hostResult.Value);
    await Sender.Send(new AddHostToDraftCommand(draftPartId, hostId, "PRIMARY"));

    return (draftPublicId, draftPartId);
  }

  private async Task AddRequiredPicksToDraftPartAsync(Guid draftPartId)
  {
    // Get the draft part to know how many picks are needed
    var draftPart = await DbContext.DraftParts
      .FirstOrDefaultAsync(dp => dp.Id == DraftPartId.Create(draftPartId));

    if (draftPart == null) return;

    var totalPicks = draftPart.TotalPicks ?? 0;

    // Add picks using direct manipulation for testing purposes
    // In a real scenario, you would use a MakePick command
    for (int position = draftPart.MinPosition!.Value; position <= draftPart.MaxPosition!.Value; position++)
    {
      // Create a movie and pick for each position
      var movie = Domain.Movies.Movie.Create(
        Faker.Random.AlphaNumeric(10),
        Faker.Random.Words(),
        Faker.Date.Past(10).Year).Value;

      DbContext.Movies.Add(movie);
      await DbContext.SaveChangesAsync();

      var participant = draftPart.Participants.First();

      var pick = Domain.Picks.Pick.Create(
        draftPartId: DraftPartId.Create(draftPartId),
        position: position,
        playOrder: position,
        playedBy: participant,
        movieId: movie.Id).Value;

      draftPart.AddPick(pick);
    }

    await DbContext.SaveChangesAsync();
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var command = new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    };

    var result = await Sender.Send(command);
    return result.Value;
  }
}

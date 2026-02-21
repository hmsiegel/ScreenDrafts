namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SetDraftPartStatusTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ------------------------------------------------------
  // Start
  // ------------------------------------------------------
  [Fact]
  public async Task SetDraftPartStatus_StartAction_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithAutoPartAndSetupAsync();

    // Act
    var result = await SendSetStatusAsync(draftPublicId, partIndex: 1, DraftPartStatusAction.Start);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetDraftPartStatus_StartAlreadyStartedPart_ShouldReturnErrorAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithAutoPartAndSetupAsync();
    await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Start);

    // Act — start again
    var result = await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_StartWithoutParticipants_ShouldReturnErrorAsync()
  {
    // Arrange — draft with a part but no participants or host
    var draftPublicId = await CreateDraftWithAutoPartAsync();

    // Act
    var result = await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_StartWithoutHost_ShouldReturnErrorAsync()
  {
    // Arrange — draft with participants but no host
    var draftPublicId = await CreateDraftWithParticipantsButNoHostAsync();

    // Act
    var result = await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  // ------------------------------------------------------
  // Complete
  // ------------------------------------------------------
  [Fact]
  public async Task SetDraftPartStatus_CompleteAction_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPublicId, draftPartId) = await CreateDraftWithAutoPartAndSetupAsync();
    await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Start);
    await AddAllPicksToDraftPartAsync(draftPartId);

    // Act
    var result = await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Complete);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Should().NotBeNull();
  }

  [Fact]
  public async Task SetDraftPartStatus_CompleteWithoutStart_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithAutoPartAsync();

    // Act
    var result = await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Complete);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_CompleteWithMissingPicks_ShouldReturnErrorAsync()
  {
    // Arrange — started but no picks added
    var (draftPublicId, _) = await CreateDraftWithAutoPartAndSetupAsync();
    await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Start);

    // Act
    var result = await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Complete);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_CompleteAlreadyCompletedPart_ShouldReturnErrorAsync()
  {
    // Arrange
    var (draftPublicId, draftPartId) = await CreateDraftWithAutoPartAndSetupAsync();
    await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Start);
    await AddAllPicksToDraftPartAsync(draftPartId);
    await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Complete);

    // Act — complete again
    var result = await SendSetStatusAsync(draftPublicId, 1, DraftPartStatusAction.Complete);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }


  // -------------------------------------------------------------------------
  // Guard — bad input
  // -------------------------------------------------------------------------

  [Fact]
  public async Task SetDraftPartStatus_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Act
    var result = await SendSetStatusAsync(Faker.Random.AlphaNumeric(10), 1, DraftPartStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithInvalidPartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithAutoPartAsync();

    // Act
    var result = await SendSetStatusAsync(draftPublicId, 999, DraftPartStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetDraftPartStatus_WithZeroPartIndex_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftWithAutoPartAsync();

    // Act
    var result = await SendSetStatusAsync(draftPublicId, 0, DraftPartStatusAction.Start);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  // ------------------------------------------------------
  // Helpers
  // ------------------------------------------------------

  private async Task<Result> SendSetStatusAsync(string draftPublicId, int partIndex, DraftPartStatusAction action)
  {
    var command = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = new SetDraftPartStatusRequest
      {
        DraftPublicId = draftPublicId,
        PartIndex = partIndex,
        Action = action
      }
    };

    return await Sender.Send(command);
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

  private async Task<string> CreateDraftWithParticipantsButNoHostAsync()
  {
    var draftPublicId = await CreateDraftWithAutoPartAsync();

    // Get the draft part ID
    var draftPartId = await GetFirstDraftPartIdAsync(draftPublicId);

    // Create and add participants (need at least 2)
    var peopleFactory = new PeopleFactory(Sender, Faker);
    for (int i = 0; i < 2; i++)
    {
      var personId = await peopleFactory.CreateAndSavePersonAsync();
      var drafterResult = await Sender.Send(new CreateDrafterCommand(personId));
      var drafterPublicId = drafterResult.Value;
      await Sender.Send(new AddParticipantToDraftPartCommand
      {
        DraftPartId = draftPartId,
        ParticipantPublicId = drafterPublicId,
        ParticipantKind = ParticipantKind.Drafter
      });
    }
    return draftPublicId;
  }

  private async Task<(string draftPublicId, Guid draftPartId)> CreateDraftWithAutoPartAndSetupAsync()
  {
    var draftPublicId = await CreateDraftWithAutoPartAsync();

    // Get the draft part ID
    var draftPartId = await GetFirstDraftPartIdAsync(draftPublicId);

    // Create and add participants (need at least 2)
    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1Result = await Sender.Send(new CreateDrafterCommand(person1Id));
    var drafter1PublicId = drafter1Result.Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2Result = await Sender.Send(new CreateDrafterCommand(person2Id));
    var drafter2PublicId = drafter2Result.Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    // Create and add a host
    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostResult = await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId });
    var hostPublicId = hostResult.Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    return (draftPublicId, draftPartId);
  }

  private async Task AddAllPicksToDraftPartAsync(Guid draftPartId)
  {
    // Get the draft part to know how many picks are needed
    var draftPart = await DbContext.DraftParts
      .FirstOrDefaultAsync(dp => dp.Id == DraftPartId.Create(draftPartId));

    if (draftPart == null) return;

    // Add picks using direct manipulation for testing purposes
    // In a real scenario, you would use a MakePick command
    for (int position = draftPart.MinPosition!.Value; position <= draftPart.MaxPosition!.Value; position++)
    {
      // Create a movie and pick for each position
      var movie = Movie.Create(Faker.Company.CompanyName(), Faker.Random.AlphaNumeric(10), Guid.NewGuid()).Value;

      DbContext.Movies.Add(movie);
      await DbContext.SaveChangesAsync();

      var participant = draftPart.Participants.First();
      var participantPublicId = await GetFirstParticipantPublicIdAsync(draftPartId);

      var command = new PlayPickCommand
      {
        DraftPartId = draftPartId,
        Position = position,
        PlayOrder = position,
        ParticipantPublicId = participantPublicId,
        ParticipantKind = participant.Kind,
        MovieId = movie.Id,
        MovieVersionName = null
      };

      await Sender.Send(command);
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

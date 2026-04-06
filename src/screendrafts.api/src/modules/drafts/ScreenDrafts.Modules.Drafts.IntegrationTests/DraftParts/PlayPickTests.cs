namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class PlayPickTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
  }

  [Fact]
  public async Task PlayPick_ShouldPersistPickInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    };

    // Act
    await Sender.Send(command);

    // Assert
    var pick = await DbContext.Picks
      .FirstOrDefaultAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId);

    pick.Should().NotBeNull();
    pick!.MovieId.Should().Be(movie.Id);
    pick.Position.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var movie = await CreateMovieAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new PlayPickCommand
    {
      DraftPartId = Faker.Random.AlphaNumeric(10),
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — movie not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WithNonExistentMovie_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();

    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = $"m_" + Faker.Random.AlphaNumeric(15)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — participant not in draft part
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WithParticipantNotInDraftPart_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    // Create a drafter who is NOT added to the draft part
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var outsiderPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = outsiderPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not started
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WhenDraftPartNotStarted_ShouldFailAsync()
  {
    // Arrange — create draft part and participant, but do NOT start the draft
    var (draftPartPublicId, drafterPublicId) = await SetupDraftPartWithoutStartingAsync();
    var movie = await CreateMovieAsync();

    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — duplicate position
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WithDuplicatePosition_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie1 = await CreateMovieAsync();
    var movie2 = await CreateMovieAsync();

    // Play position 1 first time
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie1.PublicId
    });

    // Try to play position 1 again with a different movie
    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 2,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie2.PublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — duplicate movie in the same part
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlayPick_WithSameMovieTwice_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    // Play the movie at position 1
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    });

    // Try to play the same movie at position 2
    var command = new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 2,
      PlayOrder = 2,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie.PublicId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string Drafter1PublicId, string Drafter2PublicId)> SetupStartedDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));
    var draftPartPublicId = draftPart.PublicId;

    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1PublicId = (await Sender.Send(new CreateDrafterCommand(person1Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId })).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    await Sender.Send(new SetDraftPartStatusCommand
    {      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    });

    return (draftPartPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<(string DraftPartPublicId, string DrafterPublicId)> SetupDraftPartWithoutStartingAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));
    var draftPartPublicId = draftPart.PublicId;

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var drafterPublicId = (await Sender.Send(new CreateDrafterCommand(personId))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    return (draftPartPublicId, drafterPublicId);
  }

  private async Task<Movie> CreateMovieAsync()
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), $"m_{Faker.Random.AlphaNumeric(21)}", MediaType.Movie, Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync();
    return movie;
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

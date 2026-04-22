namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class PlaySubDraftPickTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlaySubDraftPick_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, _) = await SetupActiveSubDraftAsync();
    var movie = await CreateMovieAsync();

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      MoviePublicId = movie.PublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
  }

  [Fact]
  public async Task PlaySubDraftPick_ShouldPersistPickInDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, _) = await SetupActiveSubDraftAsync();
    var movie = await CreateMovieAsync();

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      MoviePublicId = movie.PublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    var pick = await DbContext.Picks
      .FirstOrDefaultAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

    pick.Should().NotBeNull();
    pick!.MovieId.Should().Be(movie.Id);
    pick.Position.Should().Be(1);
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlaySubDraftPick_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var movie = await CreateMovieAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = Faker.Random.AlphaNumeric(10),
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}",
      MoviePublicId = movie.PublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — sub-draft not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlaySubDraftPick_WithNonExistentSubDraft_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, _, drafter1PublicId, _) = await SetupActiveSubDraftAsync();
    var movie = await CreateMovieAsync();

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = $"sd_{Faker.Random.AlphaNumeric(21)}",
      MoviePublicId = movie.PublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — sub-draft not active (pending)
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlaySubDraftPick_WhenSubDraftIsPending_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, _) = await SetupPendingSubDraftAsync();
    var movie = await CreateMovieAsync();

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      MoviePublicId = movie.PublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — movie not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlaySubDraftPick_WithNonExistentMovie_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, drafter1PublicId, _) = await SetupActiveSubDraftAsync();

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      MoviePublicId = $"m_{Faker.Random.AlphaNumeric(21)}",
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — participant not in draft part
  // -------------------------------------------------------------------------

  [Fact]
  public async Task PlaySubDraftPick_WithParticipantNotInDraftPart_ShouldFailAsync()
  {
    // Arrange
    var (draftPartPublicId, subDraftPublicId, _, _) = await SetupActiveSubDraftAsync();
    var movie = await CreateMovieAsync();

    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var outsiderPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      MoviePublicId = movie.PublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = outsiderPublicId,
      ParticipantKind = ParticipantKind.Drafter
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string SubDraftPublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupActiveSubDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1PublicId = (await Sender.Send(new CreateDrafterCommand(person1Id), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId }, TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new SetDraftPartStatusCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    }, TestContext.Current.CancellationToken);

    var subDraftPublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    }, TestContext.Current.CancellationToken)).Value;

    // Activate the sub-draft via trivia assignment
    await Sender.Send(new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
      Results =
      [
        new TriviaResultEntry
        {
          ParticipantPublicId = drafter1PublicId,
          Kind = ParticipantKind.Drafter,
          Position = 1,
          QuestionsWon = 1
        }
      ]
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, subDraftPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<(string DraftPartPublicId, string SubDraftPublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupPendingSubDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateSpeedDraftAsync(seriesId);
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1PublicId = (await Sender.Send(new CreateDrafterCommand(person1Id), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id), TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    }, TestContext.Current.CancellationToken);

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId }, TestContext.Current.CancellationToken)).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new SetDraftPartStatusCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    }, TestContext.Current.CancellationToken);

    var subDraftPublicId = (await Sender.Send(new AddSubDraftCommand
    {
      DraftPartPublicId = draftPartPublicId,
      Index = 0
    }, TestContext.Current.CancellationToken)).Value;

    // Sub-draft remains Pending (no trivia assignment)
    return (draftPartPublicId, subDraftPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<Movie> CreateMovieAsync()
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), $"m_{Faker.Random.AlphaNumeric(21)}", MediaType.Movie, Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
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
      DefaultDraftType = DraftType.SpeedDraft.Value
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<string> CreateSpeedDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.SpeedDraft.Value,
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

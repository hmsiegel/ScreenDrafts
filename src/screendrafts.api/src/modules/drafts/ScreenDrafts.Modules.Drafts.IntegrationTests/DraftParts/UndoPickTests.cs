namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class UndoPickTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task UndoPick_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    var command = new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task UndoPick_ShouldRemovePickFromDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    // Verify pick exists before undo
    var pickExists = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId);
    pickExists.Should().BeTrue();

    // Act
    await Sender.Send(new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    });

    // Assert — pick is gone
    var pickAfterUndo = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId);
    pickAfterUndo.Should().BeFalse();
  }

  [Fact]
  public async Task UndoPick_ShouldOnlyRemoveTargetPickAsync()
  {
    // Arrange — play two picks, undo only the first
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie1 = await CreateMovieAsync();
    var movie2 = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie1.Id
    });

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 2,
      PlayOrder = 2,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie2.Id
    });

    // Act — undo pick 1 only
    await Sender.Send(new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    });

    // Assert — pick 1 gone, pick 2 still present
    var pick1Exists = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId);
    var pick2Exists = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 2 && p.DraftPart.PublicId == draftPartPublicId);

    pick1Exists.Should().BeFalse();
    pick2Exists.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task UndoPick_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var command = new UndoPickCommand
    {
      DraftPartPublicId = Faker.Random.AlphaNumeric(10),
      PlayOrder = 1
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — pick not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task UndoPick_WithNonExistentPlayOrder_ShouldFailAsync()
  {
    // Arrange — started draft part but no pick at play order 99
    var (draftPartPublicId, _, _) = await SetupStartedDraftPartAsync();

    var command = new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 99
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — cannot undo a vetoed pick
  // -------------------------------------------------------------------------

  [Fact]
  public async Task UndoPick_WhenPickIsVetoed_ShouldFailAsync()
  {
    // Arrange — play a pick, then veto it
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    });

    var command = new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — cannot undo a commissioner-overridden pick
  // -------------------------------------------------------------------------

  [Fact]
  public async Task UndoPick_WhenPickIsCommissionerOverridden_ShouldFailAsync()
  {
    // Arrange — play a pick, apply veto, then commissioner-override it
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    // Veto is a prerequisite for commissioner override
    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    });

    await Sender.Send(new ApplyCommissionerOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1
    });

    var command = new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string Drafter1PublicId, string Drafter2PublicId)>
    SetupStartedDraftPartAsync()
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
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = draftPartPublicId,
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
    {
      SetDraftPartStatusRequest = new SetDraftPartStatusRequest
      {
        DraftPublicId = draftPublicId,
        PartIndex = 1,
        Action = DraftPartStatusAction.Start
      }
    });

    return (draftPartPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<Movie> CreateMovieAsync()
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), Faker.Random.AlphaNumeric(10), Guid.NewGuid()).Value;
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

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
      MoviePublicId = movie.PublicId
    }, TestContext.Current.CancellationToken);

    var command = new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
      MoviePublicId = movie.PublicId
    }, TestContext.Current.CancellationToken);

    // Verify pick exists before undo
    var pickExists = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);
    pickExists.Should().BeTrue();

    // Act
    await Sender.Send(new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    }, TestContext.Current.CancellationToken);

    // Assert — pick is gone
    var pickAfterUndo = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);
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
      MoviePublicId = movie1.PublicId
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 2,
      PlayOrder = 2,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MoviePublicId = movie2.PublicId
    }, TestContext.Current.CancellationToken);

    // Act — undo pick 1 only
    await Sender.Send(new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    }, TestContext.Current.CancellationToken);

    // Assert — pick 1 gone, pick 2 still present
    var pick1Exists = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 1 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);
    var pick2Exists = await DbContext.Picks
      .AnyAsync(p => p.PlayOrder == 2 && p.DraftPart.PublicId == draftPartPublicId, TestContext.Current.CancellationToken);

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
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
      MoviePublicId = movie.PublicId
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    }, TestContext.Current.CancellationToken);

    var command = new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
      MoviePublicId = movie.PublicId
    }, TestContext.Current.CancellationToken);

    // Veto is a prerequisite for commissioner override
    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new ApplyCommissionerOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1
    }, TestContext.Current.CancellationToken);

    var command = new UndoPickCommand
    {
      DraftPartPublicId = draftPartPublicId,
      PlayOrder = 1
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

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
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId), TestContext.Current.CancellationToken);
    var draftPartPublicId = draftPart.PublicId;

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
    {      DraftPublicId = draftPublicId,
      PartIndex = 1,
      Action = DraftPartStatusAction.Start
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, drafter1PublicId, drafter2PublicId);
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

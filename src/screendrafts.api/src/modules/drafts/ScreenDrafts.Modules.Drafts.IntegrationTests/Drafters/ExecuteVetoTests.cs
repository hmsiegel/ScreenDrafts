namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public class ExecuteVetoTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task ExecuteVeto_WhenValidData_ShouldExecuteVetoAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));

    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      DraftId: draftId.Value,
      DrafterTeamId: null,
      Position: 7,
      MovieId: movieId.Value,
      DrafterId: drafter.Id.Value, PlayOrder: 1));

    pickId.IsSuccess.Should().BeTrue();

    var command = new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      pickId.Value,
      draft.Value.Id);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var veto = await Sender.Send(new GetVetoQuery(result.Value));

    veto.Value.Should().NotBeNull();
    veto.Value.Id.Should().Be(result.Value);
    veto.Value.PickId.Should().Be(pickId.Value);
    veto.Value.DrafterId.Should().Be(drafter.Id.Value);
  }

  [Fact]
  public async Task ExecuteVeto_WhenInvalidDrafterId_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));

    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      DraftId: draftId.Value,
      DrafterTeamId: null,
      Position: 7,
      MovieId: movieId.Value,
      DrafterId: drafter.Id.Value, PlayOrder: 1));

    var drafterId = DrafterId.Create(Guid.NewGuid()).Value;

    var command = new ExecuteVetoCommand(
      null,
      drafterId,
      pickId.Value,
      draft.Value.Id);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafterId));
  }

  [Fact]
  public async Task ExecuteVeto_WhenInvalidPickId_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    await Sender.Send(new StartDraftCommand(draftId.Value));
    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));

    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var invalidPickId = Guid.NewGuid();

    var command = new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      invalidPickId,
      draft.Value.Id);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(PickErrors.NotFound(invalidPickId));
  }

  [Fact]
  public async Task ExecuteVeto_WhenInvalidDraftId_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));

    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      DraftId: draftId.Value,
      DrafterTeamId: null,
      Position: 7,
      MovieId: movieId.Value,
      DrafterId: drafter.Id.Value, PlayOrder: 1));

    var invalidDraftId = Guid.NewGuid();

    var command = new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      pickId.Value,
      invalidDraftId);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.NotFound(invalidDraftId));
  }

  [Fact]
  public async Task ExecuteVeto_WhenVetoAlreadyExists_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));
    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      DraftId: draftId.Value,
      DrafterTeamId: null,
      Position: 7,
      MovieId: movieId.Value,
      DrafterId: drafter.Id.Value, PlayOrder: 1));

    var command = new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      pickId.Value,
      draft.Value.Id);

    await Sender.Send(command);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(VetoErrors.VetoAlreadyUsed);
  }

  [Fact]
  public async Task ExecuteVeto_WhenDraftIsNotStarted_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));
    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var pickId = await Sender.Send(new AddPickCommand(
      DraftId: draftId.Value,
      DrafterTeamId: null,
      Position: 7,
      MovieId: movieId.Value,
      DrafterId: drafter.Id.Value, PlayOrder: 1));

    await Sender.Send(new PauseDraftCommand(draftId.Value));

    var command = new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      pickId.Value,
      draft.Value.Id);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.CannotVetoUnlessTheDraftIsStarted);
  }
}

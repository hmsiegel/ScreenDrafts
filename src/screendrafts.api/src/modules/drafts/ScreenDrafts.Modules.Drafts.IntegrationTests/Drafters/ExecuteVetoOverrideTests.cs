namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafters;

public class ExecuteVetoOverrideTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task ExecuteVetoOverride_WhenValidData_ShouldExecuteVetoOverrideAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();

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
      DrafterId: drafter.Id.Value,
      PlayOrder: 1));

    // Act
    var vetoId = await Sender.Send(new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      pickId.Value,
      draft.Value.Id));

    var command = new ExecuteVetoOverrideCommand(drafter.Id.Value, null, vetoId.Value);

    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var vetoOverride = await Sender.Send(new GetVetoOverrideQuery(result.Value));

    vetoOverride.Value.Should().NotBeNull();
    vetoOverride.Value.Id.Should().Be(result.Value);
  }

  [Fact]
  public async Task ExecuteVetoOverride_WhenInvalidDrafterId_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();
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
      DrafterId: drafter.Id.Value,
      PlayOrder: 1));
    var vetoId = await Sender.Send(new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      pickId.Value,
      draft.Value.Id));
    var drafterId = DrafterId.Create(Guid.NewGuid());
    var command = new ExecuteVetoOverrideCommand(drafterId.Value, null, vetoId.Value);
    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafterId.Value));
  }

  [Fact]
  public async Task ExecuteVetoOverride_WhenInvalidVetoId_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var draft = await Sender.Send(new GetDraftQuery(draftId.Value));
    var drafter = drafters[Faker.Random.Int(0, draft.Value.TotalDrafters - 1)];

    var vetoId = VetoId.Create(Guid.NewGuid());
    var command = new ExecuteVetoOverrideCommand(drafter.Id.Value, null, vetoId.Value);

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(VetoErrors.NotFound(vetoId.Value));
  }

  [Fact]
  public async Task ExecuteVetoOverride_WhenVetoOverrideAlreadyExists_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync();
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
      DrafterId: drafter.Id.Value,
      PlayOrder: 1));

    var vetoId = await Sender.Send(new ExecuteVetoCommand(
      null,
      drafter.Id.Value,
      pickId.Value,
      draft.Value.Id));

    var command = new ExecuteVetoOverrideCommand(drafter.Id.Value, null, vetoId.Value);
    await Sender.Send(command);

    // Act
    var result = await Sender.Send(command);
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(VetoErrors.VetoOverrideAlreadyUsed);
  }
}

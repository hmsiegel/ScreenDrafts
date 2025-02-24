namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class AddPickTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task AddPick_ShouldAddPickToDraftAsync()
  {
    // Arrange
    var (draftId, drafters) = await SetupDraftAndDraftersAsync();

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movieId.Value,
     drafters[0].Id.Value);

    // Act
    var result = await Sender.Send(addPickCommand);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var getDraftPicksQuery = new GetDraftPicksByDraftQuery(reloadedDraftResult.Value.Id);
    var draftPicks = await Sender.Send(getDraftPicksQuery);

    draftPicks.Value.Should().HaveCount(1);
    draftPicks.Value.First().Position.Should().Be(1);
    draftPicks.Value.First().MovieId.Should().Be(movieId.Value);
    draftPicks.Value.First().DrafterId.Should().Be(drafters[0].Id.Value);
  }


  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenDrafterIsNotAssignedToDraftAsync()
  {
    // Arrange
    var (draftId, _) = await SetupDraftAndDraftersAsync();

    var drafter = DrafterFactory.CreateDrafter();

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movieId.Value,
     drafter.Id.Value);

    // Act
    var result = await Sender.Send(addPickCommand);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafter.Id.Value));
  }

  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenMovieIsNotInDatabaseAsync()
  {
    var (draftId, drafters) = await SetupDraftAndDraftersAsync();

    var movie = MovieFactory.CreateMovie().Value;

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movie.Id,
     drafters[0].Id.Value);

    // Act
    var result = await Sender.Send(addPickCommand);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.MovieNotFound(movie.Id));
  }

  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenDraftIsNotStartedAsync()
  {
    // Arrange
    var (draftId, drafters) = await SetupDraftAndDraftersAsync();

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.MovieTitle));
    var addPickCommand = new AddPickCommand(
      draftId.Value,
      1,
      movieId.Value,
      drafters[0].Id.Value);
    // Act
    var result = await Sender.Send(addPickCommand);
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.DraftNotStarted);
  }

  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenPickPositionIsOutOfRangeAsync()
  {
    // Arrange
    var (draftId, drafters) = await SetupDraftAndDraftersAsync();

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      0,
      movieId.Value,
      drafters[0].Id.Value);

    // Act
    var result = await Sender.Send(addPickCommand);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.PickPositionIsOutOfRange);
  }

  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenPickPositionIsAlreadyTakenAsync()
  {
    // Arrange
    var (draftId, drafters) = await SetupDraftAndDraftersAsync();

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movieId.Value,
      drafters[0].Id.Value);
    await Sender.Send(addPickCommand);

    var addPickCommand2 = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movieId.Value,
      drafters[1].Id.Value);

    // Act
    var result = await Sender.Send(addPickCommand2);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.PickPositionAlreadyTaken(1));
  }

  private async Task<(Result<Guid> draftId, List<Drafter> drafters)> SetupDraftAndDraftersAsync()
  {
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));

    var drafters = new List<Drafter>();
    var hosts = new List<Host>();

    for (var i = 0; i < draft.TotalDrafters; i++)
    {
      var drafter = DrafterFactory.CreateDrafter();
      var drafterId = await Sender.Send(new CreateDrafterCommand(
        drafter.UserId,
        Name: drafter.Name));
      var addedDrafterId = await Sender.Send(new AddDrafterToDraftCommand(
        draftId.Value,
        drafterId.Value));
      var addedDrafter = await Sender.Send(new GetDrafterQuery(addedDrafterId.Value));

      drafters.Add(Drafter.Create(
        name: addedDrafter.Value.Name,
        id: DrafterId.Create(addedDrafter.Value.Id)).Value);
    }

    for (var i = 0; i < draft.TotalHosts; i++)
    {
      var host = HostsFactory.CreateHost().Value;
      var hostId = await Sender.Send(new CreateHostWithoutUserCommand(
        host.HostName));
      var addedHostId = await Sender.Send(new AddHostToDraftCommand(
        draftId.Value,
        hostId.Value));
      var addedHost = await Sender.Send(new GetHostQuery(addedHostId.Value));
      hosts.Add(Host.Create(
        addedHost.Value.Name,
        id: HostId.Create(addedHost.Value.Id)).Value);
    }

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var draftPositions = await Sender.Send(query);
    var draftPositionsList = draftPositions.Value.ToList();

    for (var i = 0; i < draft.TotalDrafters; i++)
    {
      var drafterId = drafters[i].Id;
      var draftPositionId = draftPositionsList[i].Id;
      var command = new AssignDraftPositionCommand(
        draftId.Value,
        drafterId.Value,
        draftPositionId);
      await Sender.Send(command);
    }

    return (draftId, drafters);
  }
}

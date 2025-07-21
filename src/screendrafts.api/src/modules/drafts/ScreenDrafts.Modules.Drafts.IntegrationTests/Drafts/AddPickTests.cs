namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public class AddPickTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task AddPick_ShouldAddPickToDraftAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      Faker.Random.Int(1, reloadedDraftResult.Value.TotalPicks),
      movieId.Value,
      1,
     drafters[0].Id.Value,
     null);

    // Act
    var result = await Sender.Send(addPickCommand);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var getDraftPicksQuery = new GetDraftPicksByDraftQuery(reloadedDraftResult.Value.Id);
    var draftPicks = await Sender.Send(getDraftPicksQuery);

    draftPicks.Value.Should().HaveCount(1);
    draftPicks.Value[0].Position.Should().Be(addPickCommand.Position);
    draftPicks.Value[0].MovieId.Should().Be(movieId.Value);
    draftPicks.Value[0].DrafterId.Should().Be(drafters[0].Id.Value);
  }


  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenDrafterIsNotAssignedToDraftAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var drafterFactory = new DrafterFactory(Sender, Faker);
    var drafterId = await drafterFactory.CreateAndSaveDrafterAsync();

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movieId.Value,
      1,
     drafterId,
     null);

    // Act
    var result = await Sender.Send(addPickCommand);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(drafterId, null));
  }

  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenMovieIsNotInDatabaseAsync()
  {
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var movie = MovieFactory.CreateMovie().Value;

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movie.Id,
      1,
     drafters[0].Id.Value,
     null);

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
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));
    var addPickCommand = new AddPickCommand(
      draftId.Value,
      1,
      movieId.Value,
      1,
      drafters[0].Id.Value,
      null);
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
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      0,
      movieId.Value,
      1,
      drafters[0].Id.Value, null);

    // Act
    var result = await Sender.Send(addPickCommand);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(PickErrors.PickPositionIsOutOfRange);
  }

  [Fact]
  public async Task AddPick_ShouldNotAddPickToDraft_WhenPickPositionIsAlreadyTakenAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var movie = MovieFactory.CreateMovie().Value;
    var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    var addPickCommand = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movieId.Value,
      1,
      drafters[0].Id.Value, null);
    await Sender.Send(addPickCommand);

    var addPickCommand2 = new AddPickCommand(
      reloadedDraftResult.Value.Id,
      1,
      movieId.Value,
      1,
      drafters[1].Id.Value, null);

    // Act
    var result = await Sender.Send(addPickCommand2);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.PickPositionAlreadyTaken(1));
  }

  [Fact]
  public async Task AddMultiplePicks_ShouldAddMultiplePicksToDraftAsync()
  {
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    await Sender.Send(new StartDraftCommand(draftId.Value));

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    var gameBoard = await Sender.Send(new GetGameBoardWithDraftPositionsQuery(draftId.Value));

    var draftPositions = await Sender.Send(new GetDraftPositionsByGameBoardQuery(gameBoard.Value.Id));

    var draftPositionsList = draftPositions.Value.ToList();

    List<DraftPickResponse> picks = [];

    for (int i = 0; i < reloadedDraftResult.Value.TotalPicks; i++)
    {
      var currentPickNumber = reloadedDraftResult.Value.TotalPicks - i;
      var currentPlayOrder = reloadedDraftResult.Value.TotalPicks - reloadedDraftResult.Value.TotalPicks + 1;
      Drafter? currentDrafter = null;

      foreach (var draftPosition in draftPositionsList)
      {
        if (draftPosition.Picks.Split(',').Select(int.Parse).Contains(currentPickNumber))
        {
          currentDrafter = drafters.First(d => d.Id.Value == draftPosition.DrafterId);
          break;
        }
      }

      var movie = MovieFactory.CreateMovie().Value;
      var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

      var addPickCommand = new AddPickCommand(
        reloadedDraftResult.Value.Id,
        currentPickNumber,
        movieId.Value,
        1,
        currentDrafter!.Id.Value, null);
      await Sender.Send(addPickCommand);

      picks.Add(new DraftPickResponse(
        currentPickNumber,
        currentPlayOrder,
        movieId.Value,
        movie.MovieTitle,
        currentDrafter.Id.Value,
        currentDrafter.Person.DisplayName));
    }

    var getDraftPicksQuery = new GetDraftPicksByDraftQuery(reloadedDraftResult.Value.Id);
    var draftPicks = await Sender.Send(getDraftPicksQuery);
    draftPicks.IsSuccess.Should().BeTrue();

    draftPicks.Value.Should().HaveCount(reloadedDraftResult.Value.TotalPicks);
    draftPicks.Value.Should().BeEquivalentTo(picks);
  }
}

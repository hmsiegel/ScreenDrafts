namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Commands;

public class DraftLifecycleTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task PauseDraft_WithValidDraft_ShouldSucceedAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    await Sender.Send(new StartDraftCommand(draftId.Value));

    // Act
    var result = await Sender.Send(new PauseDraftCommand(draftId.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();

    var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));
    updatedDraft.Value.DraftStatus.Should().Be(DraftStatus.Paused);
  }

  [Fact]
  public async Task PauseDraft_WithInvalidDraft_ShouldReturnFailureAsync()
  {
    // Arrange
    var (draftId, _, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);
    await Sender.Send(new StartDraftCommand(draftId.Value));

    // Act
    var result = await Sender.Send(new PauseDraftCommand(Guid.NewGuid()));

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CompleteDraft_WithValidDraft_ShouldSucceedAsync()
  {
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    // Act
    var command = new StartDraftCommand(draftId.Value);
    await Sender.Send(command);

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    for (var i = 0; i < reloadedDraftResult.Value.TotalPicks; i++)
    {
      var movie = MovieFactory.CreateMovie().Value;
      var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));


      await Sender.Send(new AddPickCommand(
        reloadedDraftResult.Value.Id,
        reloadedDraftResult.Value.TotalPicks - i,
        movieId.Value,
        i + 1,
        drafters[0].Id.Value, 
        null));
    }

    // Act
    var result = await Sender.Send(new CompleteDraftCommand(draftId.Value));

    // Assert
    result.IsSuccess.Should().BeTrue();

    var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));
    updatedDraft.Value.DraftStatus.Should().Be(DraftStatus.Completed);
  }

  [Fact]
  public async Task CompleteDraft_WithInvalidDraft_ShouldReturnFailureAsync()
  {
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    // Act
    var command = new StartDraftCommand(draftId.Value);
    await Sender.Send(command);

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    for (var i = 0; i < reloadedDraftResult.Value.TotalPicks; i++)
    {
      var movie = MovieFactory.CreateMovie().Value;
      var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));


      await Sender.Send(new AddPickCommand(
        reloadedDraftResult.Value.Id,
        reloadedDraftResult.Value.TotalPicks - i,
        movieId.Value,
        i + 1,
        drafters[0].Id.Value, 
        null));
    }

    // Act
    var result = await Sender.Send(new CompleteDraftCommand(Guid.NewGuid()));

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}

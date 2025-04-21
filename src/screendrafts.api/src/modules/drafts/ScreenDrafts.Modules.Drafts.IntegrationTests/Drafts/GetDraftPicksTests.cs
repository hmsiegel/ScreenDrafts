namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class GetDraftPicksTests(IntegrationTestWebAppFactory factory)
  : BaseIntegrationTest(factory)
{
  [Fact]
  public async Task GetDraftPicks_ReturnsDraftPicksAsync()
  {
    // Arrange
    var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));

    await Sender.Send(new StartDraftCommand(reloadedDraftResult.Value.Id));

    for (int i = 0; i < reloadedDraftResult.Value.TotalDrafters - 1; i++)
    {
      var movie = MovieFactory.CreateMovie().Value;
      var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

      var addPickCommand = new AddPickCommand(
        reloadedDraftResult.Value.Id,
        Faker.Random.Int(1, reloadedDraftResult.Value.TotalPicks),
        movieId.Value,
        1,
       drafters[i].Id.Value, null);

      await Sender.Send(addPickCommand);
    }

    // Act
    var getDraftPicksQuery = new GetDraftPicksByDraftQuery(reloadedDraftResult.Value.Id);

    var draftPicks = await Sender.Send(getDraftPicksQuery);

    // Assert
    draftPicks.IsSuccess.Should().BeTrue();
    draftPicks.Value.Should().HaveCount(reloadedDraftResult.Value.TotalDrafters - 1);
  }

  [Fact]
  public async Task GetDraftPicks_ReturnsEmptyList_WhenDraftHasNoPicksAsync()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));
    var reloadedDraftResult = await Sender.Send(new GetDraftQuery(draftId.Value));
    // Act
    var getDraftPicksQuery = new GetDraftPicksByDraftQuery(reloadedDraftResult.Value.Id);
    var draftPicks = await Sender.Send(getDraftPicksQuery);
    // Assert
    draftPicks.Errors[0].Should().Be(DraftErrors.PicksNotFound);
  }

  [Fact]
  public async Task GetDraftPicks_ReturnsError_WhenDraftDoesNotExistAsync()
  {
    // Arrange
    var draftId = Guid.NewGuid();
    // Act
    var draftPicks = await Sender.Send(new GetDraftPicksByDraftQuery(draftId));
    // Assert
    draftPicks.Errors[0].Should().Be(DraftErrors.PicksNotFound);
  }
}

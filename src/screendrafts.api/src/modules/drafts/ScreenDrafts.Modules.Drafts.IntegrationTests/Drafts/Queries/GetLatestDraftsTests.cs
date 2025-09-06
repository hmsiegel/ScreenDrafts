namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Queries;

public sealed class GetLatestDraftsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetLatestDrafts_ReturnsLatestDraftsAsync()
  {
    List<DraftResponse> drafts = [];
    do
    {
      var (draftId, drafters, _) = await SetupDraftAndDraftersAsync(DraftType.Standard);

      var reloadedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));

      var gameBoard = await Sender.Send(new GetGameBoardWithDraftPositionsQuery(draftId.Value));

      var draftPositions = await Sender.Send(new GetDraftPositionsByGameBoardQuery(gameBoard.Value.Id));

      var draftPositionsList = draftPositions.Value.ToList();
      await Sender.Send(new StartDraftCommand(draftId.Value));

      for (var j = 0; j < reloadedDraft.Value.TotalPicks; j++)
      {
        var currentPickNumber = reloadedDraft.Value.TotalPicks - j;
        Drafter? currentDrafter = null;

        foreach (var draftPosition in draftPositionsList)
        {
          if (draftPosition.Picks.Split(',').Select(int.Parse).Contains(currentPickNumber))
          {
            currentDrafter = drafters.First(d => d.Id.Value == draftPosition.DrafterId);
            break;
          }
        }

        if (currentDrafter == null)
        {
          throw new InvalidOperationException($"Could not find drafter for pick number {currentPickNumber}");
        }

        var movie = MovieFactory.CreateMovie().Value;
        var movieId = await Sender.Send(new AddMovieCommand(movie.Id, movie.ImdbId, movie.MovieTitle));

        if (movieId.IsFailure)
        {
          throw new InvalidOperationException("Could not add movie");
        }

        var addPickCommand = new AddPickCommand(
          draftId.Value,
          currentPickNumber,
          movieId.Value,
          1,
          currentDrafter!.Id.Value, null);
        await Sender.Send(addPickCommand);
      }
      await Sender.Send(new CompleteDraftCommand(draftId.Value));

      var updatedDraft = await Sender.Send(new GetDraftQuery(draftId.Value));

      drafts.Add(updatedDraft.Value);
    } while (drafts.Count < 10);

    var isPatreonOnly = true;

    var query = new GetLatestDraftsQuery(isPatreonOnly);
    var latestDraftsQueryResponse = await Sender.Send(query);

    var latestDrafts = latestDraftsQueryResponse.Value.ToList();

    latestDrafts.Should().HaveCount(5);
  }

  [Fact]
  public async Task GetLatestDrafts_WhenNoDrafts_ReturnsEmptyListAsync()
  {
    var isPatreonOnly = true;
    var query = new GetLatestDraftsQuery(isPatreonOnly);
    var latestDraftsQueryResponse = await Sender.Send(query);
    latestDraftsQueryResponse.Value.Should().BeEmpty();
  }
}

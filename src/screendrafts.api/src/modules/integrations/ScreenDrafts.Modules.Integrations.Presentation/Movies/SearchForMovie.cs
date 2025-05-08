namespace ScreenDrafts.Modules.Integrations.Presentation.Movies;

internal sealed class SearchForMovie(ISender sender) : Endpoint<MovieRequest,MovieResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/movies/search");
    Description(x => x.WithTags(Presentation.Tags.Movies));
    Policies(Presentation.Permissions.SearchMovies);
  }

  public override async Task HandleAsync(MovieRequest req, CancellationToken ct)
  {
    var query = new GetOnlineMovieCommand(req.ImdbId);

    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      var movie = result.Value;
      await SendOkAsync(movie, ct);
    }
  }
}

public sealed record MovieRequest(string ImdbId);

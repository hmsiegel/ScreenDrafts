namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMovieSummary;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetMovieSummaryRequest, GetMovieSummaryResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.GetMovieSummary);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Movies)
      .WithName(MoviesOpenApi.Names.Movies_GetMovieSummary)
      .Produces<GetMovieSummaryResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(MoviesAuth.Permissions.MoviesRead);
  }

  public override async Task HandleAsync(GetMovieSummaryRequest request, CancellationToken ct)
  {
    var query = new GetMovieSummaryQuery
    {
      ImdbId = request.ImdbId
    };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaSummary;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetMediaSummaryRequest, GetMediaSummaryResponse>
{
  public override void Configure()
  {
    Get(MoviesRoutes.GetMediaSummary);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
      .WithName(MoviesOpenApi.Names.Media_GetSummary)
      .Produces<GetMediaSummaryResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(MoviesAuth.Permissions.MediaRead);
  }

  public override async Task HandleAsync(GetMediaSummaryRequest request, CancellationToken ct)
  {
    var query = new GetMediaSummaryQuery
    {
      PublicId = request.PublicId
    };
    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}

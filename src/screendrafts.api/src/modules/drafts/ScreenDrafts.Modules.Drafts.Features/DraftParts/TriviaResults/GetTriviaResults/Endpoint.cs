namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.TriviaResults.GetTriviaResults;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetTriviaResultsRequest, GetTriviaResultsResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.TriviaResults);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_GetTriviaResults)
        .Produces<GetTriviaResultsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetTriviaResultsRequest req, CancellationToken ct)
  {
    var query = new GetTriviaResultsQuery { DraftPartId = req.DraftPartId };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

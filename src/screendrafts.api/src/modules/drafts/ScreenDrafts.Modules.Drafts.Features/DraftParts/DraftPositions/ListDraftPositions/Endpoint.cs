namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ListDraftPositions;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListDraftPositionsRequest, ListDraftPositionsResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.DraftPositions);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftPositions)
      .WithName(DraftsOpenApi.Names.DraftParts_ListDraftPositions)
      .Produces<ListDraftPositionsResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartRead);
  }

  public override async Task HandleAsync(ListDraftPositionsRequest req, CancellationToken ct)
  {
    var query = new ListDraftPositionsQuery
    {
      DraftPartId = req.DraftPartId
    };

    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, cancellationToken: ct);
  }
}

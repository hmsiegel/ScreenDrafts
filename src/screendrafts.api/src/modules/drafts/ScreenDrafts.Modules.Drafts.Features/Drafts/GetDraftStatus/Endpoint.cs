namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, Response>
{
  public override void Configure()
  {
    Get(DraftRoutes.DraftStatus);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_GetDraftStatus)
      .Produces<Response>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query
    {
      DraftPublicId = req.PublicId
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

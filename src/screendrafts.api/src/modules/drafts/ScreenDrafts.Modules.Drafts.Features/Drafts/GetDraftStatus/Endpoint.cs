namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDraftStatusRequest, Response>
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
    Permissions(DraftsAuth.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(GetDraftStatusRequest req, CancellationToken ct)
  {
    var GetDraftStatusQuery = new GetDraftStatusQuery
    {
      DraftPublicId = req.PublicId
    };

    var result = await Sender.Send(GetDraftStatusQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}



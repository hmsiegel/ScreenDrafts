namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.EndZoomSession;

internal sealed class Endpoint : ScreenDraftsEndpoint<EndZoomSessionRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.ZoomSession);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_EndZoomSession)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(EndZoomSessionRequest req, CancellationToken ct)
  {
    var command = new EndZoomSessionCommand 
    {
      DraftPartPublicId = req.DraftPartId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomSession;

internal sealed class Endpoint : ScreenDraftsEndpoint<StartZoomSessionRequest, StartZoomSessionResult>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.ZoomSession);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_StartZoomSession)
      .Produces<StartZoomSessionResult>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(StartZoomSessionRequest req, CancellationToken ct)
  {
    var hostPublicId = User.GetPublicId();

    var command = new StartZoomSessionCommand
    {
      DraftPartPublicId = req.DraftPartId,
      HostPublicId = hostPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}

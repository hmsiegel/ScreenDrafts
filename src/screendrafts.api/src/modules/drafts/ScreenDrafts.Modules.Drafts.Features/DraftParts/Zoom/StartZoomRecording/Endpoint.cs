namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StartZoomRecording;

internal sealed class Endpoint : ScreenDraftsEndpoint<StartZoomRecordingRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.StartZoomSessionRecording);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_StartZoomRecording)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(StartZoomRecordingRequest req, CancellationToken ct)
  {
    var command = new StartZoomRecordingCommand
    {
      DraftPartPublicId = req.DraftPartId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

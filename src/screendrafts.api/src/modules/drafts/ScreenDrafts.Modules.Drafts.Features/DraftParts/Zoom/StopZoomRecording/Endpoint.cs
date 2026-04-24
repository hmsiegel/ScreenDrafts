namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.StopZoomRecording;

internal sealed class Endpoint : ScreenDraftsEndpoint<StopZoomRecordingRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.StopZoomSessionRecording);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_StopZoomRecording)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(StopZoomRecordingRequest req, CancellationToken ct)
  {
    var command = new StopZoomRecordingCommand
    {
      DraftPartPublicId = req.DraftPartId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Post(DraftPartRoutes.UndoVeto);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.DraftParts_UndoVeto)
        .WithTags(DraftsOpenApi.Tags.DraftParts)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PickVetoUndo);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftPartId = Route<string>("draftPartId");
    var playOrder = Route<int>("playOrder");

    if (draftPartId is null || playOrder < 1)
    {
      await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
      return;
    }

    var command = new UndoVetoCommand { DraftPartId = draftPartId, PlayOrder = playOrder };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoVeto;

internal sealed class Endpoint : ScreenDraftsEndpoint<UndoVetoRequest>
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

  public override async Task HandleAsync(UndoVetoRequest req, CancellationToken ct)
  {
    var command = new UndoVetoCommand { DraftPartId = req.DraftPartId, PlayOrder = req.PlayOrder };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

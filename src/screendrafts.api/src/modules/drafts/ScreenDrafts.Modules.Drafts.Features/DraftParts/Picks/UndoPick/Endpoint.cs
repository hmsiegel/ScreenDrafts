namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.UndoPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<UndoPickRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.UndoPick);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_UndoPick)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.PickUndo);
  }

  public override async Task HandleAsync(UndoPickRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new UndoPickCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      PlayOrder = req.PlayOrder
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

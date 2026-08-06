namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSpeedDraftPositions;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetSpeedDraftPositionsRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SubDraftPositions);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.SubDrafts_SetPositions)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.SubDraftUpdate);
  }

  public override async Task HandleAsync(SetSpeedDraftPositionsRequest req, CancellationToken ct)
  {
    var command = new SetSpeedDraftPositionsCommand
    {
      DraftPartId = req.DraftPartId,
      Positions = req.Positions,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

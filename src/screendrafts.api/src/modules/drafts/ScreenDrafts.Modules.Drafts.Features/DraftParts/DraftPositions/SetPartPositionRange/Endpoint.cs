namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.SetPartPositionRange;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetPartPositionRangeRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.PositionRange);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_SetPartPositionRange)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(SetPartPositionRangeRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new SetPartPositionRangeCommand
    {
      DraftPartId = req.DraftPartId,
      MinimumPosition = req.MinimumPosition,
      MaximumPosition = req.MaximumPosition,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

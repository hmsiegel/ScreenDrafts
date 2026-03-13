namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.DraftPositions.ClearDraftPositionAssignment;

internal sealed class Endpoint : ScreenDraftsEndpoint<ClearDraftPositionAssignmentRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.ParticipantDraftPosition);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.DraftParts_ClearDraftPositionAssignment)
      .WithTags(DraftsOpenApi.Tags.DraftPositions)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(ClearDraftPositionAssignmentRequest req, CancellationToken ct)
  {
    var command = new ClearDraftPositionAssignmentCommand
    {
      DraftPartId = req.DraftPartId,
      PositionPublicId = req.PositionPublicId
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

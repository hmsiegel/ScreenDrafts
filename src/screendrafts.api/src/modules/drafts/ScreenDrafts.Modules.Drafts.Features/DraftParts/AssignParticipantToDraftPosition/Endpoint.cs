namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignParticipantToDraftPosition;

internal sealed class Endpoint : ScreenDraftsEndpoint<AssignParticipantToDraftPositionRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.AssignParticipantToDraftPosition);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.DraftParts_AssignParticipantToPosition)
      .WithTags(DraftsOpenApi.Tags.DraftParts)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status409Conflict);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(AssignParticipantToDraftPositionRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    if (!ParticipantKind.TryFromValue(req.ParticipantKind, out var participantKind))
    {
      AddError(r => r.ParticipantKind, "Invalid participant kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    var command = new AssignParticipantToDraftPositionCommand
    {
      DraftPartId = req.DraftPartId,
      PositionPublicId = req.PositionPublicId,
      ParticipantPublicId = req.ParticipantPublicId,
      ParticipantKind = participantKind
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

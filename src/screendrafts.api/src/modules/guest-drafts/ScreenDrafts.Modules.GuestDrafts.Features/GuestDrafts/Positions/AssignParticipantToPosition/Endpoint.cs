namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.AssignParticipantToPosition;

internal sealed class Endpoint : ScreenDraftsEndpoint<AssignParticipantToPositionRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.PositionAssign);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_AssignParticipantToPosition)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftAssignPosition);
  }

  public override async Task HandleAsync(
    AssignParticipantToPositionRequest req,
    CancellationToken ct
  )
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new AssignParticipantToPositionCommand
    {
      GuestDraftPublicId = req.PublicId,
      PositionPublicId = req.PositionPublicId,
      CallerUserPublicId = userPublicId,
      ParticipantPublicId = req.ParticipantPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

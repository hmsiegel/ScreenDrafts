namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.AddParticipant;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddParticipantRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.Participants);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_AddParticipant)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftAddParticipant);
  }

  public override async Task HandleAsync(AddParticipantRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new AddParticipantCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      GuestDrafterPublicId = req.GuestDrafterPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.RemoveParticipant;

// ── Endpoint ──────────────────────────────────────────────────────────────────
internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveParticipantRequest>
{
  public override void Configure()
  {
    Delete(GuestDraftsRoutes.ParticipantByGuestDrafter);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_RemoveParticipant)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftAddParticipant);
  }

  public override async Task HandleAsync(RemoveParticipantRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new RemoveParticipantCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      GuestDrafterPublicId = req.GuestDrafterPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

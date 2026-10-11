namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Resend;

// ── Endpoint ──────────────────────────────────────────────────────────────────
internal sealed class Endpoint : ScreenDraftsEndpoint<ResendGuestDraftInvitationRequest>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.InvitationResend);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_ResendInvitation)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftAddParticipant);
  }

  public override async Task HandleAsync(
    ResendGuestDraftInvitationRequest req,
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

    var command = new ResendGuestDraftInvitationCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      InvitationPublicId = req.InvitationPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

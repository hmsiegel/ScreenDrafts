namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.Invite;

// ── Endpoint ──────────────────────────────────────────────────────────────────
internal sealed class Endpoint
  : ScreenDraftsEndpoint<InviteGuestDrafterRequest, InviteGuestDrafterResponse>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.Invitations);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_InviteGuestDrafter)
        .Produces<InviteGuestDrafterResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftAddParticipant);
  }

  public override async Task HandleAsync(InviteGuestDrafterRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new InviteGuestDrafterCommand
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
      Email = req.Email,
      Name = req.Name,
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.List;

// ── Endpoint ──────────────────────────────────────────────────────────────────
internal sealed class Endpoint
  : ScreenDraftsEndpoint<ListGuestDraftInvitationsRequest, ListGuestDraftInvitationsResponse>
{
  public override void Configure()
  {
    Get(GuestDraftsRoutes.Invitations);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_ListInvitations)
        .Produces<ListGuestDraftInvitationsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftAddParticipant);
  }

  public override async Task HandleAsync(ListGuestDraftInvitationsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var query = new ListGuestDraftInvitationsQuery
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

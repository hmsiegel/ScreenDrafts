namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.GamePlay.GetGuestDraftGamePlay;

// ── Endpoint ──────────────────────────────────────────────────────────────

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetGuestDraftGameplayRequest, GetGuestDraftGameplayResponse>
{
  public override void Configure()
  {
    Get(GuestDraftsRoutes.ById);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_GetGameplay)
        .Produces<GetGuestDraftGameplayResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftRead);
  }

  public override async Task HandleAsync(GetGuestDraftGameplayRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var query = new GetDraftGameplayQuery
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

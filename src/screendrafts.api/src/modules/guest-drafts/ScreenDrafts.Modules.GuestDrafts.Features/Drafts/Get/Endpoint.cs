namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Get;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetGuestDraftDetailsRequest, GuestDraftDetailResponse>
{
  public override void Configure()
  {
    Get(GuestDraftsRoutes.Summary);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_GetDetails)
        .Produces<GuestDraftDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftRead);
  }

  public override async Task HandleAsync(GetGuestDraftDetailsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var query = new GetGuestDraftDetailsQuery
    {
      GuestDraftPublicId = req.PublicId,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

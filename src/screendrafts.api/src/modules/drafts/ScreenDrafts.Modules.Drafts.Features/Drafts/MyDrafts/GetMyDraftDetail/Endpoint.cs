namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDraftDetail;

// ── Endpoint ──────────────────────────────────────────────────────────────────

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetMyDraftDetailRequest, GetMyDraftDetailResponse>
{
  public override void Configure()
  {
    Get(MyDraftsRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.MyDrafts)
        .WithName(DraftsOpenApi.Names.MyDrafts_GetDetail)
        .Produces<GetMyDraftDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    });
    // Any authenticated user — handler enforces role membership.
  }

  public override async Task HandleAsync(GetMyDraftDetailRequest req, CancellationToken ct)
  {
    var userId = User.GetUserId();

    if (userId == Guid.Empty)
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var isAdmin = User.HasPermission(DraftsAuth.Permissions.DraftPartUpdate);

    var query = new GetMyDraftDetailQuery
    {
      DraftId = req.DraftId,
      UserId = userId,
      IsAdmin = isAdmin,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

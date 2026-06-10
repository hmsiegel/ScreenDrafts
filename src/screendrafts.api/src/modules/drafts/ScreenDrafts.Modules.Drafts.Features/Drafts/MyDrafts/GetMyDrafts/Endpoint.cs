namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDrafts;

// ── Endpoint ──────────────────────────────────────────────────────────────────

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetMyDraftsResponse>
{
  public override void Configure()
  {
    Get(MyDraftsRoutes.Base);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.MyDrafts)
        .WithName(DraftsOpenApi.Names.MyDrafts_GetAll)
        .Produces<GetMyDraftsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    });
    // Any authenticated user — no specific permission required.
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userId = User.GetUserId();

    if (userId == Guid.Empty)
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var isAdmin = User.HasPermission(DraftsAuth.Permissions.DraftPartUpdate);

    var query = new GetMyDraftsQuery { UserId = userId, IsAdmin = isAdmin };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

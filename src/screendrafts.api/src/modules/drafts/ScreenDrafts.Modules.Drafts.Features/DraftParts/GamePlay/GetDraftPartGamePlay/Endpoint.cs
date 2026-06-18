namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetDraftPartGameplayRequest, GetDraftPartGameplayResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.Gameplay);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.DraftParts_GetGameplay)
        .WithTags(DraftsOpenApi.Tags.DraftParts)
        .Produces<GetDraftPartGameplayResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartRead);
  }

  public override async Task HandleAsync(GetDraftPartGameplayRequest req, CancellationToken ct)
  {
    Guid? callerUserId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;

    var query = new GetDraftPartGameplayQuery
    {
      DraftPartPublicId = req.DraftPartId,
      CallerUserId = callerUserId,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

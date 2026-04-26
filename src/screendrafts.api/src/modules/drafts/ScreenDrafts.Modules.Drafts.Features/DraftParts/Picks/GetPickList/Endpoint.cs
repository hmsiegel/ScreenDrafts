namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.GetPickList;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetPickListRequest, GetPickListResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.Picks);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_PickList)
      .Produces<GetPickListResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartRead);
  }

  public override async Task HandleAsync(GetPickListRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var callerPublicId = User.GetPublicId();

    if (string.IsNullOrWhiteSpace(callerPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var query = new GetPickListQuery
    {
      DraftPartId = req.DraftPartId,
      CallerPublicId = callerPublicId,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetCampaignRequest, CampaignResponse>
{
  public override void Configure()
  {
    Get(CampaignRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Campaigns_GetCampaignById)
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .Produces<CampaignResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.CampaignRead);
  }

  public override async Task HandleAsync(GetCampaignRequest req, CancellationToken ct)
  {
    var isAdmin = User.HasPermission(DraftsAuth.Permissions.AdminViewDeleted);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var query = new GetCampaignQuery(req.PublicId, req.IncludeDeleted);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}



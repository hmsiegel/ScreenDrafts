namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListCampaignsRequest, CampaignCollectionResponse>
{
  public override void Configure()
  {
    Get(CampaignRoutes.Campaigns);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Campaigns_ListCampaigns)
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .Produces<CampaignCollectionResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.CampaignList);
  }

  public override async Task HandleAsync(ListCampaignsRequest req, CancellationToken ct)
  {
    var isAdmin = User.HasPermission(DraftsAuth.Permissions.AdminViewDeleted);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var query = new ListCampaignsQuery(IncludeDeleted: req.IncludeDeleted);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}



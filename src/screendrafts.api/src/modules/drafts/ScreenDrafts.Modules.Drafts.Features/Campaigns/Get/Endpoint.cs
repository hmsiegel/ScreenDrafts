namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, CampaignResponse>
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
    Policies(Features.Permissions.CampaignRead);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query(req.PublicId);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

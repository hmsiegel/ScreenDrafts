namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class Endpoint : ScreenDraftsEndpoint<RestoreCampaignRequest>
{
  public override void Configure()
  {
    Post(CampaignRoutes.Restore);
    Description(b => b
      .WithName(DraftsOpenApi.Names.Campaigns_RestoreCampaign)
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound));
    Policies(DraftsAuth.Permissions.CampaignRestore);
  }

  public override async Task HandleAsync(RestoreCampaignRequest req, CancellationToken ct)
  {
    var RestoreCampaignCommand = new RestoreCampaignCommand(req.PublicId);

    var result = await Sender.Send(RestoreCampaignCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



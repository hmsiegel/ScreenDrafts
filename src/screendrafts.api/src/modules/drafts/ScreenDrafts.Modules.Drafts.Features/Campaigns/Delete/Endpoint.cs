namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed class Endpoint : ScreenDraftsEndpoint<DeleteCampaignRequest>
{
  public override void Configure()
  {
    Delete(CampaignRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Campaigns_DeleteCampaign)
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.CampaignDelete);
  }

  public override async Task HandleAsync(DeleteCampaignRequest req, CancellationToken ct)
  {
    var DeleteCampaignCommand = new DeleteCampaignCommand(req.PublicId);

    var result = await Sender.Send(DeleteCampaignCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



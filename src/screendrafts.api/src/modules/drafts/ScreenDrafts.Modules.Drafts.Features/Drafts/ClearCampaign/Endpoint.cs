namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed class Endpoint : ScreenDraftsEndpoint<ClearCampaignDraftRequest>
{
  public override void Configure()
  {
    Delete(DraftRoutes.Campaign);
    Description(x =>
    {
      x.WithDescription("Removes a campaign from a draft.")
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .WithName(DraftsOpenApi.Names.Drafts_RemoveCampaign)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized);
    });
    Permissions(DraftsAuth.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(ClearCampaignDraftRequest req, CancellationToken ct)
  {
    var ClearCampaignDraftCommand = new ClearCampaignDraftCommand
    {
      DraftId = req.DraftId,
    };

    var result = await Sender.Send(ClearCampaignDraftCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



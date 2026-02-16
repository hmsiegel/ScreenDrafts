namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetCampaignDraftRequest>
{
  public override void Configure()
  {
    Put(DraftRoutes.Campaign);
    Description(b =>
    {
      b.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_SetCampaign)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.DrafterUpdate);
  }

  public override async Task HandleAsync(SetCampaignDraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var SetCampaignDraftCommand = new SetCampaignDraftCommand
    {
      DraftId = req.DraftId,
      CampaignId = req.CampaignId
    };

    var result = await Sender.Send(SetCampaignDraftCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



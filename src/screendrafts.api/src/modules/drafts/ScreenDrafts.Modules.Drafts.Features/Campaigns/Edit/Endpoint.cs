namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed class Endpoint : ScreenDraftsEndpoint<EditCampaignRequest>
{
  public override void Configure()
  {
    Patch(CampaignRoutes.ById);
    Description(x =>
    {
      x.WithDescription("Edits an existing campaign.")
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .WithName(DraftsOpenApi.Names.Campaigns_EditCampaign)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.CampaignUpdate);
  }

  public override async Task HandleAsync(EditCampaignRequest req, CancellationToken ct)
  {
    var EditCampaignCommand = new EditCampaignCommand
    {
      PublicId = req.PublicId,
      Name = req.Name,
      Slug = req.Slug
    };

    var result = await Sender.Send(EditCampaignCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



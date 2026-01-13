namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Permissions(Features.Permissions.CampaignUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      PublicId = req.PublicId,
      Name = req.Name,
      Slug = req.Slug
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

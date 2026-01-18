namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Policies(Features.Permissions.CampaignDelete);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command(req.PublicId);

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

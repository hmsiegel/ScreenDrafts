namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Policies(Features.Permissions.CampaignRestore);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command(req.PublicId);

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Post(CampaignRoutes.Restore);
    Description(b =>
      b.WithName(DraftsOpenApi.Names.Campaigns_RestoreCampaign)
        .WithTags(DraftsOpenApi.Tags.Campaigns)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(DraftsAuth.Permissions.CampaignRestore);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new RestoreCampaignCommand(publicId);
    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

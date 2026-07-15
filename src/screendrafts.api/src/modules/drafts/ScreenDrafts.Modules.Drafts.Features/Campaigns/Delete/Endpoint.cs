namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
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

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new DeleteCampaignCommand { PublicId = publicId };
    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

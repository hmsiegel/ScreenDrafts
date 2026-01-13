using ScreenDrafts.Common.Features.Http;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Permissions(Features.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      DraftId = req.DraftId,
    };

    var result = await Sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

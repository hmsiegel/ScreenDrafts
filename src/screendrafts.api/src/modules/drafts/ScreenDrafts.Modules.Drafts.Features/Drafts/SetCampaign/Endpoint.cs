using ScreenDrafts.Common.Features.Http;

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Policies(Features.Permissions.DrafterUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new Command
    {
      DraftId = req.DraftId,
      CampaignId = req.CampaignId
    };

    var result = await Sender.Send(command, ct);

    await this.MapNoContentResultsAsync(result, ct);
  }
}

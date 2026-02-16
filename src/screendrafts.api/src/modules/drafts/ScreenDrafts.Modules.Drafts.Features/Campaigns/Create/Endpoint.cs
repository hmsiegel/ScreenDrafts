using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateCampaignRequest, CreatedResponse>
{
  public override void Configure()
  {
    Post(CampaignRoutes.Campaigns);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Campaigns)
      .WithName(DraftsOpenApi.Names.Campaigns_CreateCampaign)
      .Produces<CreatedResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.CampaignCreate);
  }

  public override async Task HandleAsync(CreateCampaignRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var CreateCampaignCommand = new CreateCampaignCommand
    {
      Name = req.Name,
      Slug = req.Slug
    };
    var result = await Sender.Send(CreateCampaignCommand, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => CampaignLocations.ById(created.PublicId),
      ct);
  }
}



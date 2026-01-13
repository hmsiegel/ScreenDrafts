namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, CreatedResponse>
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
    Permissions(Features.Permissions.CampaignCreate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new Command
    {
      Name = req.Name,
      Slug = req.Slug
    };
    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => CampaignLocations.ById(created.PublicId),
      ct);
  }
}

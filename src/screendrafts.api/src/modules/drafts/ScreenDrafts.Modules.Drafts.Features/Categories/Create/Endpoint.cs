namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, CreatedResponse>
{
  public override void Configure()
  {
    Post(CategoryRoutes.Category);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Categories)
      .WithName(DraftsOpenApi.Names.Categories_CreateCategory)
      .Produces<CreatedResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.CampaignCreate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new Command
    {
      Name = req.Name,
      Description = req.Description
    };
    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => CategoryLocations.ById(created.PublicId),
      ct);
  }
}

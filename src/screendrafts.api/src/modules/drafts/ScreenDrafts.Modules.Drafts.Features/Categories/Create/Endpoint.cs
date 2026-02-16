using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateCategoryRequest, CreatedResponse>
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
    Policies(DraftsAuth.Permissions.CampaignCreate);
  }

  public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var CreateCategoryCommand = new CreateCategoryCommand
    {
      Name = req.Name,
      Description = req.Description
    };
    var result = await Sender.Send(CreateCategoryCommand, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => CategoryLocations.ById(created.PublicId),
      ct);
  }
}



namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<CategoryCollectionResponse>
{
  public override void Configure()
  {
    Get(CategoryRoutes.Category);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Categories_ListCategories)
      .WithTags(DraftsOpenApi.Tags.Categories)
      .Produces<CategoryCollectionResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.CampaignList);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new Query();

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListCategoriesRequest, CategoryCollectionResponse>
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
    Policies(DraftsAuth.Permissions.CampaignList);
  }

  public override async Task HandleAsync(ListCategoriesRequest req, CancellationToken ct)
  {
    var isAdmin = User.HasPermission(DraftsAuth.Permissions.AdminViewDeleted);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var query = new ListCategoriesQuery(IncludeDeleted: req.IncludeDeleted);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}




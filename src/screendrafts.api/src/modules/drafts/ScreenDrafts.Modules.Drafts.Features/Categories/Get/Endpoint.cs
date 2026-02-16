namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetCategoryRequest, CategoryResponse>
{
  public override void Configure()
  {
    Get(CategoryRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Categories_GetCategoryById)  
      .WithTags(DraftsOpenApi.Tags.Categories)
      .Produces<CategoryResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(DraftsAuth.Permissions.CategoryRead);
  }

  public override async Task HandleAsync(GetCategoryRequest req, CancellationToken ct)
  {
    var GetCategoryQuery = new GetCategoryQuery(req.PublicId);

    var result = await Sender.Send(GetCategoryQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}



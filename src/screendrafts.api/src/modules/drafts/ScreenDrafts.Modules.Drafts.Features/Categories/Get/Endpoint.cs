namespace ScreenDrafts.Modules.Drafts.Features.Categories.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, CategoryResponse>
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
    Permissions(Features.Permissions.CategoryRead);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var query = new Query(req.PublicId);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed class Endpoint : ScreenDraftsEndpoint<RestoreCategoryRequest>
{
  public override void Configure()
  {
    Post(CategoryRoutes.Restore);
    Description(b => b
      .WithName(DraftsOpenApi.Names.Categories_RestoreCategory)
      .WithTags(DraftsOpenApi.Tags.Categories)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound));
  }

  public override async Task HandleAsync(RestoreCategoryRequest req, CancellationToken ct)
  {
    var RestoreCategoryCommand = new RestoreCategoryCommand(req.PublicId);

    var result = await Sender.Send(RestoreCategoryCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



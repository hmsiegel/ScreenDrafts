namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed class Endpoint : ScreenDraftsEndpoint<EditCategoryRequest>
{
  public override void Configure()
  {
    Patch(CategoryRoutes.ById);
    Description(x =>
    {
      x.WithDescription("Edits an existing category.")
      .WithTags(DraftsOpenApi.Tags.Categories)
      .WithName(DraftsOpenApi.Names.Categories_EditCategory)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Permissions(DraftsAuth.Permissions.CategoryUpdate);
  }

  public override async Task HandleAsync(EditCategoryRequest req, CancellationToken ct)
  {
    var EditCategoryCommand = new EditCategoryCommand
    {
      PublicId = req.PublicId,
      Name = req.Name,
      Description = req.Description
    };

    var result = await Sender.Send(EditCategoryCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



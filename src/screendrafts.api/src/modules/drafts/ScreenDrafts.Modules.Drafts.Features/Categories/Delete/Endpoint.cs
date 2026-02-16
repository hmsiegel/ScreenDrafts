namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed class Endpoint : ScreenDraftsEndpoint<DeleteCategoryRequest>
{
  public override void Configure()
  {
    Delete(CategoryRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Categories_DeleteCategory)
      .WithTags(DraftsOpenApi.Tags.Categories)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Permissions(DraftsAuth.Permissions.CampaignDelete);
  }

  public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
  {
    var DeleteCategoryCommand = new DeleteCategoryCommand(req.PublicId);

    var result = await Sender.Send(DeleteCategoryCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}



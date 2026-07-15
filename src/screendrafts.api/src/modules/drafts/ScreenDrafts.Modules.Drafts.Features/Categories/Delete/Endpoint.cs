namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
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
    Policies(DraftsAuth.Permissions.CategoryDelete);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new DeleteCategoryCommand { PublicId = publicId };
    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Post(CategoryRoutes.Restore);
    Description(b =>
      b.WithName(DraftsOpenApi.Names.Categories_RestoreCategory)
        .WithTags(DraftsOpenApi.Tags.Categories)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(DraftsAuth.Permissions.CategoryRestore);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new RestoreCategoryCommand(publicId);
    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

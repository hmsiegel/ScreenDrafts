namespace ScreenDrafts.Modules.Drafts.Features.Categories.Edit;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Permissions(Features.Permissions.CategoryUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      PublicId = req.PublicId,
      Name = req.Name,
      Description = req.Description
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Categories.Delete;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Permissions(Features.Permissions.CampaignDelete);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command(req.PublicId);

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

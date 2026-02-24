namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetCategoriesDraftRequest>
{
  public override void Configure()
  {
    Put(DraftRoutes.Categories);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Categories)
      .WithName(DraftsOpenApi.Names.Drafts_SetCategories)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status401Unauthorized);
    });
    Policies(DraftsAuth.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(SetCategoriesDraftRequest req, CancellationToken ct)
  {
    var command = new SetCategoriesDraftCommand
    {
      DraftId = req.DraftId,
      CategoryIds = req.CategoryIds
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

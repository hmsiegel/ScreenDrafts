namespace ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveCategoryFromDraftRequest>
{
  public override void Configure()
  {
    Delete(DraftRoutes.CategoryById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_RemoveCategory)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.CategoryDelete);
  }

  public override async Task HandleAsync(RemoveCategoryFromDraftRequest req, CancellationToken ct)
  {
    var command = new RemoveCategoryFromDraftCommand
    {
      DraftId = req.DraftId,
      CategoryId = req.CategoryId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

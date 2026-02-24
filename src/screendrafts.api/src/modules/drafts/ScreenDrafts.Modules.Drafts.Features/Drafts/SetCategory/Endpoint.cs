namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategory;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetCategoryDraftRequest>
{
  public override void Configure()
  {
    Put(DraftRoutes.Category);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_SetCategory)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(DraftsAuth.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(SetCategoryDraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new SetCategoryDraftCommand
    {
      DraftId = req.DraftId,
      CategoryId = req.CategoryId
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);

  }
}

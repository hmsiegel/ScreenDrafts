namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Delete;

internal sealed class Endpoint : ScreenDraftsEndpoint<DeleteDraftRequest>
{
  public override void Configure()
  {
    Delete("/drafts/{publicId}");
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_DeleteDraft)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    // GUESS — verify the actual permission constant for draft management.
    // Only confirmed reference point I have is DraftsAuth.Permissions.PredictionManage
    // from the predictions feature; this is named by inference, not confirmed.
    Policies(DraftsAuth.Permissions.DraftDelete);
  }

  public override async Task HandleAsync(DeleteDraftRequest req, CancellationToken ct)
  {
    var command = new DeleteDraftCommand { PublicId = req.PublicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

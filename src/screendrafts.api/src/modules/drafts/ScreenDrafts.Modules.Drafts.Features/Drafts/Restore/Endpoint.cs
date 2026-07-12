namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Restore;

internal sealed class Endpoint : ScreenDraftsEndpoint<RestoreDraftRequest>
{
  public override void Configure()
  {
    Post("/drafts/{publicId}/restore");
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_Restore)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    });
    // Same guess as DeleteDraftEndpoint — verify the real permission constant.
    Policies(DraftsAuth.Permissions.DraftUpdate);
  }

  public override async Task HandleAsync(RestoreDraftRequest req, CancellationToken ct)
  {
    var command = new RestoreDraftCommand { PublicId = req.PublicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

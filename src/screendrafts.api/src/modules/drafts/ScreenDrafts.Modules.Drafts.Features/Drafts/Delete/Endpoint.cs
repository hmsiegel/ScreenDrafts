namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Delete;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Delete(DraftRoutes.ById);
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

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new DeleteDraftCommand { PublicId = publicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Restore;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Post(DraftRoutes.Restore);
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
    Policies(DraftsAuth.Permissions.DraftRestore);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new RestoreDraftCommand { PublicId = publicId };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

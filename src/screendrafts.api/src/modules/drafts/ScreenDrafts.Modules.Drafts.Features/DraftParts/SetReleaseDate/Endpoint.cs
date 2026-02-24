namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetReleaseDateRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.Releases);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_SetReleaseDate)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }
  public override async Task HandleAsync(SetReleaseDateRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new SetReleaseDateCommand
    {
      DraftPartId = req.DraftPartId,
      ReleaseDate = req.ReleaseDate,
      ReleaseChannel = ReleaseChannel.FromValue(req.ReleaseChannel)
    };
    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}

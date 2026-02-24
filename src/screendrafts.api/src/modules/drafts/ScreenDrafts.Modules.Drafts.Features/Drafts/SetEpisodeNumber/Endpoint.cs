namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetEpisodeNumber;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetEpisodeNumberRequest>
{
  public override void Configure()
  {
    Put(DraftRoutes.Episode);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Drafts_SetEpisodeNumber)
      .WithTags(DraftsOpenApi.Tags.Drafts)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftUpdate);
  }
  public override async Task HandleAsync(SetEpisodeNumberRequest req, CancellationToken ct)
  {
    var command = new SetEpisodeNumberCommand
    {
      DraftId = req.DraftId,
      ReleaseChannel = ReleaseChannel.FromValue(req.ReleaseChannel),
      EpisodeNumber = req.EpisodeNumber
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}

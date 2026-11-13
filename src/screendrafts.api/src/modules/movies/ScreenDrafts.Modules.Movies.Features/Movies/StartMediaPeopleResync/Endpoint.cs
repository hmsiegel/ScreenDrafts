using ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.StartMediaPeopleResync;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<StartMediaPeopleResyncRequest, MediaPeopleResyncStatusResponse>
{
  public override void Configure()
  {
    Post(MediaPeopleResyncRoutes.Base);
    // Same permission as adding media: this is an admin-only maintenance action.
    Policies(MoviesAuth.Permissions.MediaCreate);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MediaPeopleResyncRoutes.StartName)
        .Produces<MediaPeopleResyncStatusResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status409Conflict);
    });
  }

  public override async Task HandleAsync(StartMediaPeopleResyncRequest req, CancellationToken ct)
  {
    var result = await Sender.Send(new StartMediaPeopleResyncCommand { Limit = req.Limit }, ct);
    await this.SendOkAsync(result, ct);
  }
}

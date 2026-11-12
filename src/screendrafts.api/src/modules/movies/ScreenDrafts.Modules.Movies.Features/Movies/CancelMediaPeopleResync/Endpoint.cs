using ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.CancelMediaPeopleResync;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<MediaPeopleResyncStatusResponse>
{
  public override void Configure()
  {
    Post(MediaPeopleResyncRoutes.Cancel);
    Policies(MoviesAuth.Permissions.MediaCreate);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MediaPeopleResyncRoutes.CancelName)
        .Produces<MediaPeopleResyncStatusResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await Sender.Send(new CancelMediaPeopleResyncCommand(), ct);
    await this.SendOkAsync(result, ct);
  }
}

using ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPeopleResyncStatus;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<MediaPeopleResyncStatusResponse>
{
  public override void Configure()
  {
    Get(MediaPeopleResyncRoutes.Base);
    Policies(MoviesAuth.Permissions.MediaCreate);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MediaPeopleResyncRoutes.StatusName)
        .Produces<MediaPeopleResyncStatusResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await Sender.Send(new GetMediaPeopleResyncStatusQuery(), ct);
    await this.SendOkAsync(result, ct);
  }
}

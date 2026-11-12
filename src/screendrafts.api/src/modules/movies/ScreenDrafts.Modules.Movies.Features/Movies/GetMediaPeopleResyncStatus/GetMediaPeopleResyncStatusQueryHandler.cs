using ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPeopleResyncStatus;

internal sealed class GetMediaPeopleResyncStatusQueryHandler(
  MediaPeopleResyncRunner runner,
  IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetMediaPeopleResyncStatusQuery, MediaPeopleResyncStatusResponse>
{
  private readonly MediaPeopleResyncRunner _runner = runner;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<MediaPeopleResyncStatusResponse>> Handle(
    GetMediaPeopleResyncStatusQuery request,
    CancellationToken cancellationToken
  )
  {
    var status = _runner.GetStatus();

    if (status.State == MediaPeopleResyncRunner.Running)
    {
      return Result.Success(status);
    }

    // No run in progress: say how much a new one would do, so it can be sized before starting.
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);
    var pending = await connection.ExecuteScalarAsync<int>(
      MediaPeopleResyncSql.Count,
      new { VideoGame = MediaType.VideoGame.Value, MusicVideo = MediaType.MusicVideo.Value }
    );

    return Result.Success(status with { Pending = pending });
  }
}

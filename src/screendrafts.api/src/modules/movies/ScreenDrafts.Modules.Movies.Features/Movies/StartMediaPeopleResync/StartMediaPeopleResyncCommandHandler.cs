using ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.StartMediaPeopleResync;

internal sealed class StartMediaPeopleResyncCommandHandler(MediaPeopleResyncRunner runner)
  : ICommandHandler<StartMediaPeopleResyncCommand, MediaPeopleResyncStatusResponse>
{
  private readonly MediaPeopleResyncRunner _runner = runner;

  public Task<Result<MediaPeopleResyncStatusResponse>> Handle(
    StartMediaPeopleResyncCommand request,
    CancellationToken cancellationToken
  )
  {
    if (request.Limit is <= 0)
    {
      return Task.FromResult(
        Result.Failure<MediaPeopleResyncStatusResponse>(MediaPeopleResyncErrors.InvalidLimit)
      );
    }

    if (!_runner.TryStart(request.Limit))
    {
      return Task.FromResult(
        Result.Failure<MediaPeopleResyncStatusResponse>(MediaPeopleResyncErrors.AlreadyRunning)
      );
    }

    return Task.FromResult(Result.Success(_runner.GetStatus()));
  }
}

using ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.CancelMediaPeopleResync;

internal sealed class CancelMediaPeopleResyncCommandHandler(MediaPeopleResyncRunner runner)
  : ICommandHandler<CancelMediaPeopleResyncCommand, MediaPeopleResyncStatusResponse>
{
  private readonly MediaPeopleResyncRunner _runner = runner;

  // Idempotent: cancelling when nothing runs is not an error, it just reports the status.
  public Task<Result<MediaPeopleResyncStatusResponse>> Handle(
    CancelMediaPeopleResyncCommand request,
    CancellationToken cancellationToken
  )
  {
    _runner.Cancel();
    return Task.FromResult(Result.Success(_runner.GetStatus()));
  }
}

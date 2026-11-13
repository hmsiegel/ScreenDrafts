using ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.StartMediaPeopleResync;

internal sealed record StartMediaPeopleResyncCommand : ICommand<MediaPeopleResyncStatusResponse>
{
  public int? Limit { get; init; }
}

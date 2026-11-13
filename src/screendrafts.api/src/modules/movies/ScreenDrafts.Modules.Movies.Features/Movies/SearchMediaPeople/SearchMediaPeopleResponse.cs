namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMediaPeople;

internal sealed record SearchMediaPeopleResponse
{
  public required IReadOnlyList<MediaPersonResponse> Items { get; init; }
}

namespace ScreenDrafts.Modules.Integrations.Features.People.SearchImdbPeople;

internal sealed record SearchImdbPeopleResponse
{
  public IReadOnlyList<ImdbPersonSearchResult> Results { get; init; } = [];
}

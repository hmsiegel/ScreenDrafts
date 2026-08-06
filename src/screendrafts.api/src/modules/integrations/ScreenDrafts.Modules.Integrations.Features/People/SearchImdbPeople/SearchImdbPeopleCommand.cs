namespace ScreenDrafts.Modules.Integrations.Features.People.SearchImdbPeople;

internal sealed record SearchImdbPeopleCommand : ICommand<SearchImdbPeopleResponse>
{
  public required string Query { get; init; }
}

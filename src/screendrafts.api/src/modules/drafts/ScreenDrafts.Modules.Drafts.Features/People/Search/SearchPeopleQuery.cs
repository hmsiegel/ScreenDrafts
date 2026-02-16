namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record SearchPeopleQuery(string Search, int Limit) : IQuery<PeopleSearchResponse>;


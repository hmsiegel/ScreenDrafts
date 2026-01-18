namespace ScreenDrafts.Modules.Drafts.Features.People.Search;

internal sealed record Query(string Search, int Limit) : IQuery<PeopleSearchResponse>;

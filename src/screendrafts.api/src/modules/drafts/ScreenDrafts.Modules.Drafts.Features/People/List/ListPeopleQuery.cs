namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed record ListPeopleQuery(ListPeopleRequest ListPeopleRequest) : IQuery<PeopleCollectionResponse>;



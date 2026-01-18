namespace ScreenDrafts.Modules.Drafts.Features.People.List;

internal sealed record PeopleCollectionResponse(
  PagedResult<PersonResponse> People);

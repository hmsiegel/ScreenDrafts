namespace ScreenDrafts.Modules.Drafts.Application.People.Queries.ListPeople;

public sealed record ListPeopleQuery(
  int Page,
  int PageSize,
  string? FirstName = null,
  string? LastName = null,
  string? DisplayName = null,
  bool? IsDrafter = null,
  bool? IsHost = null,
  string? Sort = null,
  string? Dir = null,
  string? Q = null) : IQuery<PagedResult<PersonResponse>>;

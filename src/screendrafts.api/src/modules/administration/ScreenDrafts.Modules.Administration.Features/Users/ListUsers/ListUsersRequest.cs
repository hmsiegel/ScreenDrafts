namespace ScreenDrafts.Modules.Administration.Features.Users.ListUsers;

internal sealed record ListUsersRequest
{
  [FromQuery(Name = "search")]
  public string? Search { get; init; }

  [FromQuery(Name = "roles")]
  public List<string>? Roles { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; }

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; }
}

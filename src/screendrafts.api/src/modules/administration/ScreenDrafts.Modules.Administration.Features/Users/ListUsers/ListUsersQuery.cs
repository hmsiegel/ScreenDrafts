using ScreenDrafts.Common.Presentation.Responses;

namespace ScreenDrafts.Modules.Administration.Features.Users.ListUsers;

internal sealed record ListUsersQuery : IQuery<PagedResult<UserItem>>
{
  public string? Search { get; init; }
  public int Page { get; init; }
  public int PageSize { get; init; }
}

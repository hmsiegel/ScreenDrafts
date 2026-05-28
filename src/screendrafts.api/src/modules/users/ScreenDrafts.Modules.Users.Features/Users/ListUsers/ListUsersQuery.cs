namespace ScreenDrafts.Modules.Users.Features.Users.ListUsers;

internal sealed record ListUsersQuery : IQuery<ListUsersResponse>
{
  public string? Search { get; init; }
}

namespace ScreenDrafts.Modules.Users.Features.Users.ListUsers;

internal sealed record ListUsersResponse
{
  public IReadOnlyList<ListUsersItem> Users { get; init; } = [];
}

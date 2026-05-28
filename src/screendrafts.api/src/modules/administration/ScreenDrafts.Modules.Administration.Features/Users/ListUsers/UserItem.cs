namespace ScreenDrafts.Modules.Administration.Features.Users.ListUsers;

internal sealed record UserItem
{
  public string PublicId { get; init; } = default!;
  public string DisplayName { get; init; } = default!;
  public string Email { get; init; } = default!;
  public IReadOnlyList<string> Roles { get; init; } = [];
}

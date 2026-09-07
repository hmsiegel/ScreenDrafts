namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersByIds;

internal sealed record GetUsersByIdsResponse
{
  public IReadOnlyList<GetByUserIdResponse> Users { get; init; } = [];
}

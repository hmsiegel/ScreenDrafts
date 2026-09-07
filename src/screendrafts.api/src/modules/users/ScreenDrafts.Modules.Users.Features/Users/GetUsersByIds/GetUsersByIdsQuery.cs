namespace ScreenDrafts.Modules.Users.Features.Users.GetUsersByIds;

internal sealed record GetUsersByIdsQuery(IReadOnlyList<Guid> UserIds)
  : IQuery<GetUsersByIdsResponse>;

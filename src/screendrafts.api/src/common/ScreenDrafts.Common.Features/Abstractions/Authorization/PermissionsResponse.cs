namespace ScreenDrafts.Common.Features.Abstractions.Authorization;

public sealed record PermissionsResponse(Guid UserId, HashSet<string> Permissions);


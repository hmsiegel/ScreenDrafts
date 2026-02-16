namespace ScreenDrafts.Common.Abstractions.Authorization;

public sealed record PermissionsResponse(Guid UserId, HashSet<string> Permissions);


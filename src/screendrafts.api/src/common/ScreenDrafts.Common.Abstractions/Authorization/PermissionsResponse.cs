namespace ScreenDrafts.Common.Abstractions.Authorization;

public sealed record PermissionsResponse(Guid UserId, string PublicId, HashSet<string> Permissions);


namespace ScreenDrafts.Modules.Users.Features.Admin.GetPermissionByCode;

internal sealed record Query(string Code) : IQuery<Response>;

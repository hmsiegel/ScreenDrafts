namespace ScreenDrafts.Modules.Users.Features.Admin.GetPermissionByCode;

internal sealed record GetPermissionByCodeQuery(string Code) : IQuery<Response>;

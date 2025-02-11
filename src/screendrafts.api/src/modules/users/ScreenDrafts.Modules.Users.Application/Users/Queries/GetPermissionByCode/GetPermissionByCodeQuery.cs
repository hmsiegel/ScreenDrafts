namespace ScreenDrafts.Modules.Users.Application.Users.Queries.GetPermissionByCode;

public sealed record GetPermissionByCodeQuery(string Code) : IQuery<PermissionResponse>;

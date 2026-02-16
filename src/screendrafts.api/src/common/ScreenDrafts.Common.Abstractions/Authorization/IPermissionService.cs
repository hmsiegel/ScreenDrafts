namespace ScreenDrafts.Common.Abstractions.Authorization;

public interface IPermissionService
{
  Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}


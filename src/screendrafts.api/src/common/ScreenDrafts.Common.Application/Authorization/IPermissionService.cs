namespace ScreenDrafts.Common.Application.Authorization;

public interface IPermissionService
{
  Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}


namespace ScreenDrafts.Common.Features.Abstractions.Authorization;

public interface IPermissionService
{
  Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}


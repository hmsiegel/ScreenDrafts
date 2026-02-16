using ScreenDrafts.Common.Abstractions.Authorization;

namespace ScreenDrafts.Modules.Users.Features.Authorization;

internal sealed class PermissionService(ISender sender) : IPermissionService
{
  private readonly ISender _sender = sender;

  public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
  {
    return await _sender.Send(new Users.GetUserPermissions.GetUserPermissionsQuery(identityId));
  }
}

using ScreenDrafts.Common.Presentation.Http.Authentication;

namespace ScreenDrafts.Common.Presentation.Http.Authorization;

internal sealed class CustomClaimsTransformation(IServiceScopeFactory serviceScopeFactory) : IClaimsTransformation
{
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

  public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    if (principal.HasClaim(c => c.Type == CustomClaims.Permission))
    {
      return principal;
    }

    using var scope = _serviceScopeFactory.CreateScope();

    var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();

    var identityId = principal.GetIdentityId();

    var result = await permissionService.GetUserPermissionsAsync(identityId);

    if (result.IsFailure)
    {
      throw new ScreenDraftsException(nameof(IPermissionService.GetUserPermissionsAsync), result.Error);
    }

    var claimsIdentity = new ClaimsIdentity();

    claimsIdentity.AddClaim(new Claim(CustomClaims.Sub, result.Value.UserId.ToString()));
    claimsIdentity.AddClaim(new Claim(CustomClaims.PublicId, result.Value.PublicId));

    foreach (var permission in result.Value.Permissions)
    {
      claimsIdentity.AddClaim(new Claim(CustomClaims.Permission, permission));
    }

    principal.AddIdentity(claimsIdentity);

    return principal;
  }
}

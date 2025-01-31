namespace ScreenDrafts.Common.Infrastructure.Authorization;

internal sealed class CustomClaimsTransformation(IServiceScopeFactory serviceScopeFactory) : IClaimsTransformation
{
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

  public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
  {
    if (principal.HasClaim(c => c.Type ==  CustomClaims.Sub))
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

    foreach (var permission in result.Value.Permissions)
    {
      claimsIdentity.AddClaim(new Claim(CustomClaims.Permission, permission));
    }

    principal.AddIdentity(claimsIdentity);

    return principal;
  }
}

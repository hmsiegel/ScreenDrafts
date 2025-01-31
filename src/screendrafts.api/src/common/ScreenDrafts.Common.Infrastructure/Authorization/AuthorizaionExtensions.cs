namespace ScreenDrafts.Common.Infrastructure.Authorization;

internal static class AuthorizaionExtensions
{
  internal static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
  {
    services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();
    services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
    services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
    return services;
  }
}

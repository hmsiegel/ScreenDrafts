namespace ScreenDrafts.Common.Infrastructure.Authentication;

internal static class AuthenticationExtensions
{
  internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
  {
    services.AddAuthorization(options =>
    {
      options.FallbackPolicy = null;
    });

    services.AddAuthentication().AddJwtBearer();

    services.AddHttpContextAccessor();

    services.ConfigureOptions<JwtBearerConfigureOptions>();

    return services;
  }
}

namespace ScreenDrafts.Common.Infrastructure.Identity;

public static class KeyCloakServiceExtension
{
  public static IServiceCollection AddKeyCloakClient(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);
    services.Configure<KeyCloakOptions>(configuration.GetSection("Users:KeyCloak"));

    services.AddTransient<KeyCloakAuthDelegatingHandler>();

    services
      .AddHttpClient<KeyCloakClient>(
        (sp, client) =>
        {
          var keyCloakOptions = sp.GetRequiredService<IOptions<KeyCloakOptions>>().Value;

          client.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
        }
      )
      .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();
    return services;
  }
}

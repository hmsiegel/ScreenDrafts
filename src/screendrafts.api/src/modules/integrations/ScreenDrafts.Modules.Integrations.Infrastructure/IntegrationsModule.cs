namespace ScreenDrafts.Modules.Integrations.Infrastructure;

public static class IntegrationsModule
{
  public static IServiceCollection AddIntegrationsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {

    services.AddInfrastructure(configuration);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    // Will implement this later.
  }
}

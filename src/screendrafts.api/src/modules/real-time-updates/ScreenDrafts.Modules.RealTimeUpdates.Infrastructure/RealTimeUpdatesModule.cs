namespace ScreenDrafts.Modules.RealTimeUpdates.Infrastructure;

public static class RealTimeUpdatesModule
{
  public static IServiceCollection AddRealTimeUpdatesModule(
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

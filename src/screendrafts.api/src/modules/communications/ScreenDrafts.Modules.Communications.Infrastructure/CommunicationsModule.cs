namespace ScreenDrafts.Modules.Communications.Infrastructure;

public static class CommunicationsModule
{
  public static IServiceCollection AddCommunicationsModule(
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

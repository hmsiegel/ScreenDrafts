namespace ScreenDrafts.Modules.Administration.Infrastructure;

public static class AdministrationModule
{
  public static IServiceCollection AddAdministrationModule(
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

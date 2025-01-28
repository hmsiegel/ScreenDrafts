namespace ScreenDrafts.Modules.Audit.Infrastructure;

public static class AuditModule
{
  public static IServiceCollection AddAuditModule(
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

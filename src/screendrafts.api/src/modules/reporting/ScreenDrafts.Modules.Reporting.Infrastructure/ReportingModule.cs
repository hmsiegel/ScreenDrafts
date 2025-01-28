namespace ScreenDrafts.Modules.Reporting.Infrastructure;

public static class ReportingModule
{
  public static IServiceCollection AddReportingModule(
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

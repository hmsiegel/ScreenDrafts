namespace ScreenDrafts.Modules.Reporting.Infrastructure;

public static class ReportingInfrastructure
{
  private const string ModuleName = "Reporting";

  public static IServiceCollection AddReportingInfratsructure(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<ReportingDbContext>(
      (sp, options) =>
      {
        options.UseModuleDefaults(ModuleName, Schemas.Reporting, sp);
      }
    );

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ReportingDbContext>());

    services.AddMemoryCache();

    services.Configure<OutboxOptions>(configuration.GetSection("Reporting:Outbox"));
    services.ConfigureOptions<ConfigureProcessOutboxJob>();
    services.Configure<InboxOptions>(configuration.GetSection("Reporting:Inbox"));
    services.ConfigureOptions<ConfigureProcessInboxJob>();
    services.ConfigureOptions<ConfigureWeeklySpotlightRotationJob>();

    return services;
  }
}

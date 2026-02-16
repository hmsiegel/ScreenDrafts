namespace ScreenDrafts.Modules.Reporting.Infrastructure;

public static class ReportingInfrastructure
{
  private const string ModuleName = "Reporting";

  public static IServiceCollection AddReportingInfratsructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<ReportingDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Reporting, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ReportingDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Administration:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Administration:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

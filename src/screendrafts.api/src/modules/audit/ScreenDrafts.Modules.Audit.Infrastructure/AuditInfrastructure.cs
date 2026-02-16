namespace ScreenDrafts.Modules.Audit.Infrastructure;

public static class AuditInfrastructure
{
  private const string ModuleName = "Audit";

  public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<AuditDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Audit, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuditDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Audit:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Audit:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

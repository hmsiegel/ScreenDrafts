using ScreenDrafts.Modules.Audit.Infrastructure.Keycloak;

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

    services.AddSingleton<AuditPreProcessor>();
    services.AddSingleton<AuditPostProcessor>();

    services.AddScoped<IAuditWriteService, AuditWriteService>();

    services.AddSingleton<MongoAuditRepository>();

    services.Configure<KeycloakPollerOptions>(configuration.GetSection(KeycloakPollerOptions.SectionName));

    services.ConfigureOptions<ConfigureKeycloakPollerJob>();

    services.Configure<OutboxOptions>(configuration.GetSection("Audit:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Audit:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }

  public static EndpointDefinition AddAuditProcessors(this EndpointDefinition ep)
  {
    ArgumentNullException.ThrowIfNull(ep);

    ep.PreProcessor<AuditPreProcessor>(Order.Before);
    ep.PostProcessor<AuditPostProcessor>(Order.After);

    return ep;
  }
}

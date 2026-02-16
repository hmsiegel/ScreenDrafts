namespace ScreenDrafts.Modules.Integrations.Infrastructure;

public static class IntegrationsInfrastructure
{
  private const string ModuleName = "Integrations";

  public static IServiceCollection AddIntegrationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<IntegrationsDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Integrations, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IntegrationsDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Integrations:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Integrations:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    services.AddOptions<ImdbSettings>()
      .Bind(configuration.GetSection("Integrations:Imdb"))
      .ValidateDataAnnotations();


    services.AddOptions<OmdbSettings>()
      .Bind(configuration.GetSection("Integrations:Omdb"))
      .ValidateDataAnnotations();


    return services;
  }
}

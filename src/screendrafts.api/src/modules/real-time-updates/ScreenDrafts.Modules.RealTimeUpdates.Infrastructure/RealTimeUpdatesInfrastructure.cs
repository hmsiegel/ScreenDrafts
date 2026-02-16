namespace ScreenDrafts.Modules.RealTimeUpdates.Infrastructure;

public static class RealTimeUpdatesInfrastructure
{
  private const string ModuleName = "RealTimeUpdates";
  public static IServiceCollection AddRealTimeUpdatesInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<RealTimeUpdatesDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.RealTimeUpdates, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RealTimeUpdatesDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("RealTimeUpdates:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("RealTimeUpdates:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

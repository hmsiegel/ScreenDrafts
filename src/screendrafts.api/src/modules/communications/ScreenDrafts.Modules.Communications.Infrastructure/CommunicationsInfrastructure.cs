namespace ScreenDrafts.Modules.Communications.Infrastructure;

public static class CommunicationsInfrastructure
{
  private const string ModuleName = "Communications";

  public static IServiceCollection AddCommunicationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<CommunicationsDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Communications, sp);
    });


    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CommunicationsDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Communications:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Communications:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

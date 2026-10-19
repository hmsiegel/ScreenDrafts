using ScreenDrafts.Modules.Communications.Infrastructure.ContactMessages;

namespace ScreenDrafts.Modules.Communications.Infrastructure;

public static class CommunicationsInfrastructure
{
  private const string ModuleName = "Communications";

  public static IServiceCollection AddCommunicationsInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<CommunicationsDbContext>(
      (sp, options) =>
      {
        options.UseModuleDefaults(ModuleName, Schemas.Communications, sp);
      }
    );

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CommunicationsDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Communications:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Communications:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    services.AddScoped<IContactMessageRepository, ContactMessageRepository>();

    services.Configure<TurnstileOptions>(configuration.GetSection(TurnstileOptions.SectionName));
    services.AddHttpClient<ITurnstileVerifier, TurnstileVerifier>(client =>
      client.Timeout = TimeSpan.FromSeconds(5)
    );

    return services;
  }
}

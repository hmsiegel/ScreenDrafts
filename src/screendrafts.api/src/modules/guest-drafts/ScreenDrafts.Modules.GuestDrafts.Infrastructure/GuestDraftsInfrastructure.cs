namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure;

public static class GuestDraftsInfrastructure
{
  private const string ModuleName = "GuestDrafts";

  public static IServiceCollection AddGuestDraftsInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<GuestDraftsDbContext>(
      (sp, options) =>
      {
        options.UseModuleDefaults(ModuleName, Schemas.Drafts, sp);
      }
    );

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<GuestDraftsDbContext>());

    services.AddMemoryCache();

    services.Configure<OutboxOptions>(configuration.GetSection("GuestDrafts:Outbox"));
    services.ConfigureOptions<ConfigureProcessOutboxJob>();
    services.Configure<InboxOptions>(configuration.GetSection("GuestDrafts:Inbox"));
    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

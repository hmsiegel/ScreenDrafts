namespace ScreenDrafts.Modules.Drafts.Infrastructure;

public static class DraftsInfrastructure
{
  private const string ModuleName = "Drafts";
  public static IServiceCollection AddDraftsInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<DraftsDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Drafts, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DraftsDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Drafts:Outbox"));
    services.ConfigureOptions<ConfigureProcessOutboxJob>();
    services.Configure<InboxOptions>(configuration.GetSection("Drafts:Inbox"));
    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

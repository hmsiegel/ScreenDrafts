namespace ScreenDrafts.Modules.Administration.Infrastructure;

public static class AdministrationInfrastructure
{
  private const string ModuleName = "Administration";
  public static IServiceCollection AddAdministrationInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<AdministrationDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName,Schemas.Administration, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AdministrationDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Administration:Outbox"));
    services.ConfigureOptions<ConfigureProcessOutboxJob>();
    services.Configure<InboxOptions>(configuration.GetSection("Administration:Inbox"));
    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

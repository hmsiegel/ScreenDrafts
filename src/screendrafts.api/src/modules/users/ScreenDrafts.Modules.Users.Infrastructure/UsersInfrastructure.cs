namespace ScreenDrafts.Modules.Users.Infrastructure;

public static class UsersInfrastructure
{
  private const string ModuleName = "Users";

  public static IServiceCollection AddUsersInfratructure(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<UsersDbContext>(
      (sp, options) =>
      {
        options.UseModuleDefaults(ModuleName, Schemas.Users, sp);
      }
    );

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

    services.AddScoped<IEmailBootstrapTokenService, EmailBootstrapTokenService>();

    services.Configure<OutboxOptions>(configuration.GetSection("Users:Outbox"));
    services.ConfigureAllQuartzSchedulers(ConfigureProcessOutboxJob.Configure);
    services.Configure<InboxOptions>(configuration.GetSection("Users:Inbox"));
    services.ConfigureAllQuartzSchedulers(ConfigureProcessInboxJob.Configure);

    return services;
  }
}

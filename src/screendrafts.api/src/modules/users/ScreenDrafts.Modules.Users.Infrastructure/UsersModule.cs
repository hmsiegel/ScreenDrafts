namespace ScreenDrafts.Modules.Users.Infrastructure;

public static class UsersModule
{
  public static IServiceCollection AddUsersModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddInfrastructure(configuration);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Database")!;

    services.AddScoped<IPermissionService, PermissionService>();

    services.Configure<KeyCloakOptions>(configuration.GetSection("Users:KeyCloak"));

    services.AddTransient<KeyCloakAuthDelegatingHandler>();

    services
      .AddHttpClient<KeyCloakClient>((sp, client) =>
      {
        var keyCloakOptions = sp.GetRequiredService<IOptions<KeyCloakOptions>>().Value;

        client.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
      })
      .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

    services.AddTransient<IIdentityProviderService, IdentityProviderService>();

    services.AddDbContext<UsersDbContext>((sp, options) =>
      options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
      .UseSnakeCaseNamingConvention()
      .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

    services.AddScoped<IUserRepository, UserRepository>();
  }
}

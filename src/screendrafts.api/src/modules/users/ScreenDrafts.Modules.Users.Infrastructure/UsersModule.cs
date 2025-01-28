namespace ScreenDrafts.Modules.Users.Infrastructure;

public static class UsersModule
{
  public static IServiceCollection AddUsersModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {

    services.AddInfrastructure(configuration);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Database")!;

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

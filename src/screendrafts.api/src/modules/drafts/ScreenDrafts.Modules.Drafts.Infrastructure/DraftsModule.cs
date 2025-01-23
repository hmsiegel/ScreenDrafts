namespace ScreenDrafts.Modules.Drafts.Infrastructure;

public static class DraftsModule
{
  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {

    services.AddInfrastructure(configuration);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Database")!;

    services.AddDbContext<DraftsDbContext>(options =>
      options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Drafts))
      .UseSnakeCaseNamingConvention());

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DraftsDbContext>());

    services.AddScoped<IDraftsRepository, DraftsRepository>();
  }
}

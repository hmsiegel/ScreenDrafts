namespace ScreenDrafts.Modules.Drafts.Infrastructure;

public static class DraftsModule
{
  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    services.AddMediatR(config =>
    {
      config.RegisterServicesFromAssembly(Application.AssemblyReference.Assembly);
    });

    services.AddValidatorsFromAssembly(Application.AssemblyReference.Assembly);

    services.AddInfrastructure(configuration);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Database")!;

    var npgsqlDataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
    services.TryAddSingleton(npgsqlDataSource);

    services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

    services.AddDbContext<DraftsDbContext>(options =>
      options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Drafts))
      .UseSnakeCaseNamingConvention());

    services.AddScoped<IDraftsRepository, DraftsRepository>();

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DraftsDbContext>());
  }
}

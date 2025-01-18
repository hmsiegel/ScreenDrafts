namespace ScreenDrafts.Modules.Drafts.Domain;

public static class DraftsModule
{
  public static IServiceCollection AddDraftsModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {
    string connectionString = configuration.GetConnectionString("Database")!;


    services.AddDbContext<DraftsDbContext>(options =>
      options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Drafts))
      .UseSnakeCaseNamingConvention());

    return services;
  }

  public static void MapEndpoints(IEndpointRouteBuilder app)
  {
    CreateDraft.MapEndpoint(app);
    GetDraft.MapEndpoint(app);
    ListDrafts.MapEndpoint(app);
  }
}

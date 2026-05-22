namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database;

internal sealed class DraftsDbContextFactory : IDesignTimeDbContextFactory<DraftsDbContext>
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Vulnerability",
    "S2068:Credentials should not be hard-coded",
    Justification = "Dev Only"
  )]
  private const string ConnectionString =
    "Host=screendrafts.database;Port=5432;Database=screendrafts;Username=drafts_user;Password=drafts_password;Include Error Detail=true";

  public DraftsDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<DraftsDbContext>();
    optionsBuilder
      .UseNpgsql(
        ConnectionString,
        npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "drafts")
      )
      .UseSnakeCaseNamingConvention();
    return new DraftsDbContext(optionsBuilder.Options);
  }
}

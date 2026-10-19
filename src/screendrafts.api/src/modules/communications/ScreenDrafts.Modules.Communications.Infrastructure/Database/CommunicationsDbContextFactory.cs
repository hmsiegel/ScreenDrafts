namespace ScreenDrafts.Modules.Communications.Infrastructure.Database;

internal sealed class CommunicationsDbContextFactory
  : IDesignTimeDbContextFactory<CommunicationsDbContext>
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Vulnerability",
    "S2068:Credentials should not be hard-coded",
    Justification = "Dev Only"
  )]
  private const string ConnectionString =
    "Host=screendrafts.database;Port=5432;Database=screendrafts;Username=communications_user;Password=communications_password;Include Error Detail=true";

  public CommunicationsDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<CommunicationsDbContext>();
    optionsBuilder
      .UseNpgsql(
        ConnectionString,
        npgsql =>
          npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "communications")
      )
      .UseSnakeCaseNamingConvention();
    return new CommunicationsDbContext(optionsBuilder.Options);
  }
}

namespace ScreenDrafts.Modules.Reporting.Infrastructure.Database;

internal sealed class ReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Vulnerability",
    "S2068:Credentials should not be hard-coded",
    Justification = "Dev Only"
  )]
  private const string ConnectionString =
    "Host=screendrafts.database;Port=5432;Database=screendrafts;Username=reporting_user;Password=reporting_password;Include Error Detail=true";

  public ReportingDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<ReportingDbContext>();
    optionsBuilder
      .UseNpgsql(
        ConnectionString,
        npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "reporting")
      )
      .UseSnakeCaseNamingConvention();
    return new ReportingDbContext(optionsBuilder.Options);
  }
}

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Database;

internal sealed class GuestDraftsDbContextFactory
  : IDesignTimeDbContextFactory<GuestDraftsDbContext>
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Vulnerability",
    "S2068:Credentials should not be hard-coded",
    Justification = "Dev Only"
  )]
  private const string ConnectionString =
    "Host=screendrafts.database;Port=5432;Database=screendrafts;Username=guest_drafts_user;Password=guest_drafts_password;Include Error Detail=true";

  public GuestDraftsDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<GuestDraftsDbContext>();
    optionsBuilder
      .UseNpgsql(
        ConnectionString,
        npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "guest_drafts")
      )
      .UseSnakeCaseNamingConvention();
    return new GuestDraftsDbContext(optionsBuilder.Options);
  }
}

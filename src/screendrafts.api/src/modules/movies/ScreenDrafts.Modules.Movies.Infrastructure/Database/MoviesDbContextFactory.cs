using Microsoft.EntityFrameworkCore.Design;

namespace ScreenDrafts.Modules.Movies.Infrastructure.Database;

internal sealed class MoviesDbContextFactory : IDesignTimeDbContextFactory<MoviesDbContext>
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Vulnerability",
    "S2068:Credentials should not be hard-coded",
    Justification = "Dev Only"
  )]
  private const string ConnectionString =
    "Host=screendrafts.database;Port=5432;Database=screendrafts;Username=movies_user;Password=movies_password;Include Error Detail=true";

  public MoviesDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<MoviesDbContext>();
    optionsBuilder
      .UseNpgsql(
        ConnectionString,
        npgsql => npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "movies")
      )
      .UseSnakeCaseNamingConvention();
    return new MoviesDbContext(optionsBuilder.Options);
  }
}

namespace ScreenDrafts.Web.Extensions;

internal static class MigrationsExtensions
{
  internal static void ApplyMigrations(this IApplicationBuilder app)
  {
    using var scope = app.ApplicationServices.CreateScope();
    ApplyMigration<AdministrationDbContext>(scope);
    ApplyMigration<AuditDbContext>(scope);
    ApplyMigration<DraftsDbContext>(scope);
    ApplyMigration<IntegrationsDbContext>(scope);
    ApplyMigration<MoviesDbContext>(scope);
    ApplyMigration<RealTimeUpdatesDbContext>(scope);
    ApplyMigration<ReportingDbContext>(scope);
    ApplyMigration<UsersDbContext>(scope);
  }

  private static void ApplyMigration<TDbContext>(IServiceScope scope)
    where TDbContext : DbContext
  {
    using var context = scope.ServiceProvider.GetRequiredService<TDbContext>();

    context.Database.Migrate();
  }
}

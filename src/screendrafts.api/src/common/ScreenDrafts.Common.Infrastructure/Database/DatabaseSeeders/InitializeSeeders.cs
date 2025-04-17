namespace ScreenDrafts.Common.Infrastructure.Database.DatabaseSeeders;

public static class InitializeSeeders
{
  public static async Task UseSeedersAsync(this IApplicationBuilder app)
  {
    ArgumentNullException.ThrowIfNull(app);

    using var scope = app.ApplicationServices.CreateScope();
    var seeders = scope.ServiceProvider.GetServices<ICustomSeeder>();

    foreach (var seeder in seeders)
    {
      await seeder.InitializeAsync();
    }
  }
}

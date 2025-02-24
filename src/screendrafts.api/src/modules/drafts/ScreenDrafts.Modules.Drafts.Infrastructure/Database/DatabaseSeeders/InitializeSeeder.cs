namespace ScreenDrafts.Modules.Drafts.Infrastructure.Database.DatabaseSeeders;

internal static class InitializeSeeders
{
  internal static async Task UseSeedersAsync(this IApplicationBuilder app)
  {
    using var scope = app.ApplicationServices.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<ICustomSeeder>();
    await seeder.InitializeAsync();
  }
}

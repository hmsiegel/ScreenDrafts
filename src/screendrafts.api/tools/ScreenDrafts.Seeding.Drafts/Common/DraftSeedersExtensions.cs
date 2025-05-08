namespace ScreenDrafts.Seeding.Drafts.Common;

internal static class DraftSeedersExtensions
{
  public static IServiceCollection AddDraftSeeders(this IServiceCollection services)
  {
    services.Scan(scan => scan
        .FromAssemblyOf<DraftSeeder>()
        .AddClasses(classes => classes.AssignableTo<ICustomSeeder>(), false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    services.AddScoped<SeederExecutor>();

    return services;
  }
}

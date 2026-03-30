namespace ScreenDrafts.Seeding.Honorifics.Common;

internal static class HonorificsSeedersExtensions
{
  public static IServiceCollection AddHonorificsSeeders(this IServiceCollection services)
  {
    services.Scan(scan => scan
        .FromAssemblyOf<HonorificSeeder>()
        .AddClasses(classes => classes.AssignableTo<ICustomSeeder>(), false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    services.AddScoped<SeederExecutor>();

    return services;
  }
}

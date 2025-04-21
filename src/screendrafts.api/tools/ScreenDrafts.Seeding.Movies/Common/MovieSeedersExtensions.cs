namespace ScreenDrafts.Seeding.Movies.Common;

internal static class MovieSeedersExtensions
{
  public static IServiceCollection AddMovieSeeders(this IServiceCollection services)
  {
    services.Scan(scan => scan
        .FromAssemblyOf<MovieSeeder>()
        .AddClasses(classes => classes.AssignableTo<ICustomSeeder>(), false)
        .AsImplementedInterfaces()
        .WithTransientLifetime());

    services.AddScoped<SeederExecutor>();

    return services;
  }
  internal static IServiceCollection AddRepositoriesFromModules(
    this IServiceCollection services,
    Assembly[] infrastructureAssemblies)
  {
    services.Scan(scan => scan
        .FromAssemblies(infrastructureAssemblies)
        .AddClasses(classes => classes.AssignableTo<IRepository>(), false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    return services;
  }
}

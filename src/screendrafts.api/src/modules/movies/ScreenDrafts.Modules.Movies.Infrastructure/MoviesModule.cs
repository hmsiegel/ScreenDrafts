namespace ScreenDrafts.Modules.Movies.Infrastructure;

public static class MoviesModule
{
  public static IServiceCollection AddMoviesModule(
    this IServiceCollection services,
    IConfiguration configuration)
  {

    services.AddInfrastructure(configuration);

    return services;
  }

  private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    // Will implement this later.
  }
}

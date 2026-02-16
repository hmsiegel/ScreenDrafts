namespace ScreenDrafts.Modules.Movies.Infrastructure;

public static class MoviesInfrastructure
{
  private const string ModuleName = "Movies";

  public static IServiceCollection AddMoviesInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);

    services.AddDbContext<MoviesDbContext>((sp, options) =>
    {
      options.UseModuleDefaults(ModuleName, Schemas.Movies, sp);
    });

    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MoviesDbContext>());

    services.Configure<OutboxOptions>(configuration.GetSection("Movies:Outbox"));

    services.ConfigureOptions<ConfigureProcessOutboxJob>();

    services.Configure<InboxOptions>(configuration.GetSection("Movies:Inbox"));

    services.ConfigureOptions<ConfigureProcessInboxJob>();

    return services;
  }
}

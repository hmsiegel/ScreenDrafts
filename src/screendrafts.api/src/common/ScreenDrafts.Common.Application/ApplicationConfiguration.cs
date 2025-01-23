namespace ScreenDrafts.Common.Application;
public static class ApplicationConfiguration
{
  public static IServiceCollection AddApplication(this IServiceCollection services, Assembly[] moduleAssemblies)
  {

    services.AddMediatR(config =>
    {
      config.RegisterServicesFromAssemblies(moduleAssemblies);
    });

    services.AddValidatorsFromAssemblies(moduleAssemblies);

    return services;
  }
}

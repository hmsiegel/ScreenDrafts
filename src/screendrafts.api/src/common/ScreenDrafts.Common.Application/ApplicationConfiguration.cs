namespace ScreenDrafts.Common.Application;

public static class ApplicationConfiguration
{
  public static IServiceCollection AddApplication(
    this IServiceCollection services,
    Assembly[] moduleAssemblies,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
    // Fix: Use the correct extension method 'Get<T>()' from Microsoft.Extensions.Configuration.Binder
    // Ensure the NuGet package 'Microsoft.Extensions.Configuration.Binder' is referenced in your project.
    var mediatrSettings = configuration.GetSection("MediatR").Get<MediatRSettings>();

    services.AddMediatR(config =>
    {
      config.RegisterServicesFromAssemblies(moduleAssemblies);
      config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
      config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
      config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
      config.LicenseKey = mediatrSettings?.LicenseKey;
    });

    services.AddValidatorsFromAssemblies(moduleAssemblies);

    return services;
  }
}

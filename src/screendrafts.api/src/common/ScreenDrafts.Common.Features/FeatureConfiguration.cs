namespace ScreenDrafts.Common.Features;

public static class FeatureConfiguration
{
  public static IServiceCollection AddFeatures(
    this IServiceCollection services,
    Assembly[] moduleAssemblies,
    IConfiguration configuration)
  {
    ArgumentNullException.ThrowIfNull(configuration);
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

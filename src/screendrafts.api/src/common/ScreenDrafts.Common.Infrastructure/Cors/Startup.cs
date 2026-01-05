namespace ScreenDrafts.Common.Infrastructure.Cors;

internal static class Startup
{
  internal static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
  {
    const string PolicyName = "AllowUI";

    ArgumentNullException.ThrowIfNull(services);
    ArgumentNullException.ThrowIfNull(configuration);

    var corsSettings = configuration.GetSection("Cors").Get<CorsSettings>();

    if (corsSettings is null)
    {
      return services;
    }

    var origins = corsSettings.AllowedOrigins;

    services.AddCors(options =>
    {
      options.AddPolicy(
        PolicyName,
        builder =>
        {
          builder
            .WithOrigins(origins!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
    return services;
  }
}

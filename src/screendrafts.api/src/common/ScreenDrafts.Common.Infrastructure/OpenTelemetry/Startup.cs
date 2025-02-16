namespace ScreenDrafts.Common.Infrastructure.OpenTelemetry;

internal static class Startup
{
  private const string MongoDbDiagnosticSource = "MongoDB.Driver.Core.Extensions.DiagnosticSources";

  internal static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, string serviceName)
  {
    services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(serviceName))
        .WithTracing(tracing =>
        {
          tracing
                  .AddAspNetCoreInstrumentation()
                  .AddHttpClientInstrumentation()
                  .AddEntityFrameworkCoreInstrumentation()
                  .AddRedisInstrumentation()
                  .AddNpgsql()
                  .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
                  .AddSource(MongoDbDiagnosticSource);

          tracing.AddOtlpExporter();
        });

    return services;
  }
}

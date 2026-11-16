namespace ScreenDrafts.Modules.Audit.Infrastructure.Keycloak;

internal static class ConfigureKeycloakPollerJob
{
  public static void Configure(IQuartzBuilder quartz)
  {
    var jobName = typeof(KeycloakAuthEventPollerJob).FullName!;

    quartz.AddJob<KeycloakAuthEventPollerJob>(job => job.WithIdentity(jobName));
    quartz.AddTrigger<KeycloakAuthEventPollerJob>(
      (services, trigger) =>
      {
        var options = services.GetRequiredService<IOptions<KeycloakPollerOptions>>().Value;

        trigger
          .ForJob(jobName)
          .WithSimpleSchedule(x =>
            x.WithInterval(TimeSpan.FromSeconds(options.IntervalInSeconds)).RepeatForever()
          );
      }
    );
  }
}

namespace ScreenDrafts.Modules.Audit.Infrastructure.Keycloak;

internal sealed class ConfigureKeycloakPollerJob(IOptions<KeycloakPollerOptions> options)
  : IConfigureOptions<QuartzOptions>
{
  private readonly KeycloakPollerOptions _options = options.Value;

  public void Configure(QuartzOptions options)
  {
    var jobName = typeof(KeycloakAuthEventPollerJob).FullName!;

    options
      .AddJob<KeycloakAuthEventPollerJob>(configure =>
        configure.WithIdentity(jobName))
      .AddTrigger(configure =>
        configure.ForJob(jobName)
          .WithSimpleSchedule(schedule =>
            schedule.WithIntervalInSeconds(_options.IntervalInSeconds)
                    .RepeatForever()));
  }
}

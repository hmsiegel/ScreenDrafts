namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class ConfigureWeeklySpotlightRotationJob : IConfigureOptions<QuartzOptions>
{
  public void Configure(QuartzOptions options)
  {
    var jobName = typeof(WeeklySpotlightRotationJob).FullName!;

    options
      .AddJob<WeeklySpotlightRotationJob>(configure => configure.WithIdentity(jobName))
      .AddTrigger(configure =>
        configure
          .ForJob(jobName)
          .WithCronSchedule("0 0 0 ? * MON *", x => x.InTimeZone(TimeZoneInfo.Utc))
      );
  }
}

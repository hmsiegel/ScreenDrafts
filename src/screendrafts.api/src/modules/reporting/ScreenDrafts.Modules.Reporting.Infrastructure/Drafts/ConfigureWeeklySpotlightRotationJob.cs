namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal static class ConfigureWeeklySpotlightRotationJob
{
  public static void Configure(IQuartzBuilder quartz)
  {
    var jobName = typeof(WeeklySpotlightRotationJob).FullName!;

    quartz.AddJob<WeeklySpotlightRotationJob>(job => job.WithIdentity(jobName));
    quartz.AddTrigger<WeeklySpotlightRotationJob>(trigger =>
      trigger
        .ForJob(jobName)
        .WithCronSchedule(
          CronExpression.Parse("0 0 0 ? * MON *", CronFormat.Quartz),
          cron => cron.InTimeZone(TimeZoneInfo.Utc)
        )
    );
  }
}

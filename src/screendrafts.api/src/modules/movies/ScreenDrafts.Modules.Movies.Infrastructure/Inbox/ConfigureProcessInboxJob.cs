namespace ScreenDrafts.Modules.Movies.Infrastructure.Inbox;

internal static class ConfigureProcessInboxJob
{
  public static void Configure(IQuartzBuilder quartz)
  {
    var jobName = typeof(ProcessInboxJob).FullName!;

    quartz.AddJob<ProcessInboxJob>(job => job.WithIdentity(jobName));
    quartz.AddTrigger(
      (services, trigger) =>
      {
        var options = services.GetRequiredService<IOptions<InboxOptions>>().Value;
        var interval = options.IntervalInMilliseconds.HasValue
          ? TimeSpan.FromMilliseconds(options.IntervalInMilliseconds.Value)
          : TimeSpan.FromSeconds(options.IntervalInSeconds);

        trigger
          .ForJob(jobName)
          .WithSimpleSchedule(schedule => schedule.WithInterval(interval).RepeatForever());
      }
    );
  }
}

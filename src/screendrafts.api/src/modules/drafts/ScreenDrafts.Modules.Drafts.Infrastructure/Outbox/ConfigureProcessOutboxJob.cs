namespace ScreenDrafts.Modules.Drafts.Infrastructure.Outbox;

internal static class ConfigureProcessOutboxJob
{
  public static void Configure(IQuartzBuilder quartz)
  {
    var jobName = typeof(ProcessOutboxJob).FullName!;

    quartz.AddJob<ProcessOutboxJob>(job => job.WithIdentity(jobName));
    quartz.AddTrigger(
      (services, trigger) =>
      {
        var options = services.GetRequiredService<IOptions<OutboxOptions>>().Value;
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

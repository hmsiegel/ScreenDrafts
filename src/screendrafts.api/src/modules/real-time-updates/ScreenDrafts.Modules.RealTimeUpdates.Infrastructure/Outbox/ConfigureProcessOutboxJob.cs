namespace ScreenDrafts.Modules.RealTimeUpdates.Infrastructure.Outbox;

internal static class ConfigureProcessOutboxJob
{
  public static void Configure(IQuartzBuilder quartz)
  {
    var jobName = typeof(ProcessOutboxJob).FullName!;

    quartz.AddJob<ProcessOutboxJob>(job => job.WithIdentity(jobName));
    quartz.AddTrigger<ProcessOutboxJob>(
      (services, trigger) =>
      {
        var options = services.GetRequiredService<IOptions<OutboxOptions>>().Value;

        trigger.ForJob(jobName).WithSimpleSchedule(TimeSpan.FromSeconds(options.IntervalInSeconds));
      }
    );
  }
}

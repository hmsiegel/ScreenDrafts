namespace ScreenDrafts.Modules.RealTimeUpdates.Infrastructure.Inbox;

internal static class ConfigureProcessInboxJob
{
  public static void Configure(IQuartzBuilder quartz)
  {
    var jobName = typeof(ProcessInboxJob).FullName!;

    quartz.AddJob<ProcessInboxJob>(configure => configure.WithIdentity(jobName));
    quartz.AddTrigger<ProcessInboxJob>(
      (services, trigger) =>
      {
        var options = services.GetRequiredService<IOptions<InboxOptions>>().Value;

        trigger.ForJob(jobName).WithSimpleSchedule(TimeSpan.FromSeconds(options.IntervalInSeconds));
      }
    );
  }
}

namespace ScreenDrafts.Modules.Reporting.Infrastructure.Inbox;

internal sealed class ConfigureProcessInboxJob(IOptions<InboxOptions> outboxOptions) : IConfigureOptions<QuartzOptions>
{
  private readonly InboxOptions _outboxOptions = outboxOptions.Value;

  public void Configure(QuartzOptions options)
  {
    var jobName = typeof(ProcessInboxJob).FullName!;

    options
      .AddJob<ProcessInboxJob>(configure => configure.WithIdentity(jobName))
      .AddTrigger(configure =>
      configure
        .ForJob(jobName)
        .WithSimpleSchedule(schedule => schedule
          .WithIntervalInSeconds(_outboxOptions.IntervalInSeconds)
          .RepeatForever()));
  }
}

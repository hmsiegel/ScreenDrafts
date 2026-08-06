namespace ScreenDrafts.Modules.Integrations.Infrastructure.Outbox;

internal sealed class ConfigureProcessOutboxJob(IOptions<OutboxOptions> outboxOptions)
  : IConfigureOptions<QuartzOptions>
{
  private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

  public void Configure(QuartzOptions options)
  {
    var jobName = typeof(ProcessOutboxJob).FullName!;
    var interval = _outboxOptions.IntervalInMilliseconds.HasValue
      ? TimeSpan.FromMilliseconds(_outboxOptions.IntervalInMilliseconds.Value)
      : TimeSpan.FromSeconds(_outboxOptions.IntervalInSeconds);

    options
      .AddJob<ProcessOutboxJob>(configure => configure.WithIdentity(jobName))
      .AddTrigger(configure =>
        configure
          .ForJob(jobName)
          .WithSimpleSchedule(schedule => schedule.WithInterval(interval).RepeatForever())
      );
  }
}

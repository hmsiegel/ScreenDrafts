namespace ScreenDrafts.Modules.Drafts.Infrastructure.PodcastLinks;

internal sealed class ConfigureResolvePodcastLinksJob(IOptions<PodcastLinksOptions> podcastLinksOptions)
  : IConfigureOptions<QuartzOptions>
{
  private readonly PodcastLinksOptions _options = podcastLinksOptions.Value;

  public void Configure(QuartzOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);

    var jobName = typeof(ResolvePodcastLinksJob).FullName!;

    options
      .AddJob<ResolvePodcastLinksJob>(configure => configure.WithIdentity(jobName))
      .AddTrigger(configure =>
        configure
          .ForJob(jobName)
          .WithSimpleSchedule(schedule =>
            schedule
              .WithInterval(TimeSpan.FromSeconds(_options.IntervalInSeconds))
              .RepeatForever()
          )
      );
  }
}

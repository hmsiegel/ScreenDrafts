namespace ScreenDrafts.Modules.Drafts.Infrastructure.PodcastLinks;

internal static class ConfigureResolvePodcastLinksJob
{
  public static void Configure(IQuartzBuilder quartz)
  {
    var jobName = typeof(ResolvePodcastLinksJob).FullName!;

    quartz.AddJob<ResolvePodcastLinksJob>(job => job.WithIdentity(jobName));
    quartz.AddTrigger<ResolvePodcastLinksJob>(
      (services, trigger) =>
      {
        var options = services.GetRequiredService<IOptions<PodcastLinksOptions>>().Value;
        trigger
          .ForJob(jobName)
          .WithSimpleSchedule(x =>
            x.WithInterval(TimeSpan.FromSeconds(options.IntervalInSeconds)).RepeatForever()
          );
      }
    );
  }
}

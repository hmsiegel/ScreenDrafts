namespace ScreenDrafts.Modules.Reporting.Infrastructure.Drafts;

internal sealed class SpotlightRotationService(ISchedulerFactory schedulerFactory)
  : ISpotlightRotationService
{
  private readonly ISchedulerFactory _schedulerFactory = schedulerFactory;

  public async Task TriggerRotationJobAsync(CancellationToken cancellationToken = default)
  {
    var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

    var jobKey = new JobKey(typeof(WeeklySpotlightRotationJob).FullName!);

    if (!await scheduler.CheckExists(jobKey, cancellationToken))
    {
      throw new ScreenDraftsException($"Job '{jobKey}' not registered.");
    }

    await scheduler.TriggerJob(jobKey, cancellationToken);
  }
}

namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public interface ISpotlightRotationService
{
  Task TriggerRotationJobAsync(CancellationToken cancellationToken = default);
}

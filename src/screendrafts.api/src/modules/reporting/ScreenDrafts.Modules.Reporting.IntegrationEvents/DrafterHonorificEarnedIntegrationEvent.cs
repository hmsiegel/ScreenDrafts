using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Reporting.IntegrationEvents;

public sealed class DrafterHonorificEarnedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  Guid drafterIdValue,
  string draftPartPublicId,
  int previousHonorificValue,
  int newHonorificValue,
  int appearanceCount)
  : IntegrationEvent(id, occurredOnUtc)
{
  public Guid DrafterIdValue { get; set; } = drafterIdValue;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public int PreviousHonorificValue { get; set; } = previousHonorificValue;
  public int NewHonorificValue { get; set; } = newHonorificValue;
  public int AppearanceCount { get; set; } = appearanceCount;
}

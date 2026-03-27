using ScreenDrafts.Common.Application.EventBus;

namespace ScreenDrafts.Modules.Reporting.IntegrationEvents;

public sealed class MovieHonorificEarnedIntegrationEvent(
  Guid id,
  DateTime occurredOnUtc,
  string moviePublicId,
  string movieTitle,
  string draftPartPublicId,
  int previousAppearanceHonorificValue,
  int newAppearanceHonorificValue,
  int previousPositionHonorificValue,
  int newPositionHonorificValue,
  int appearanceCount)
  : IntegrationEvent(id, occurredOnUtc)
{
  public string MoviePublicId { get; set; } = moviePublicId;
  public string MovieTitle { get; set; } = movieTitle;
  public string DraftPartPublicId { get; set; } = draftPartPublicId;
  public int PreviousAppearanceHonorificValue { get; set; } = previousAppearanceHonorificValue;
  public int NewAppearanceHonorificValue { get; set; } = newAppearanceHonorificValue;
  public int PreviousPositionHonorificValue { get; set; } = previousPositionHonorificValue;
  public int NewPositionHonorificValue { get; set; } = newPositionHonorificValue;
  public int AppearanceCount { get; set; } = appearanceCount;
}

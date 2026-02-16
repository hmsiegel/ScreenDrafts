namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public readonly record struct DraftPartGamePlaySnapshot(
  int MinPosition,
  int MaxPosition,
  DraftType DraftType,
  SeriesId SeriesId)
{
  public static Result<DraftPartGamePlaySnapshot> Create(
    int minPosition,
    int maxPosition,
    DraftType draftType,
    SeriesId seriesId)
  {
    if (minPosition < 0)
    {
      return Result.Failure<DraftPartGamePlaySnapshot>(DraftPartErrors.MinimumPositionMustBeGreaterThanZero);
    }

    if (maxPosition < 0 || maxPosition < minPosition)
    {
      return Result.Failure<DraftPartGamePlaySnapshot>(DraftPartErrors.MaxPositionIsOutOfRange);
    }

    if (draftType is null)
    {
      return Result.Failure<DraftPartGamePlaySnapshot>(DraftErrors.DraftTypeIsRequired);
    }

    if (seriesId is null)
    {
      return Result.Failure<DraftPartGamePlaySnapshot>(DraftErrors.SeriesIdIsRequired);
    }

    return Result.Success(new DraftPartGamePlaySnapshot(minPosition, maxPosition, draftType, seriesId));
  }
}

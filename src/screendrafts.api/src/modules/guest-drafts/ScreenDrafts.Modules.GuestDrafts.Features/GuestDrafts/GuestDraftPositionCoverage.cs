using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts;

/// <summary>
/// Validates that a set of owner-supplied board positions exactly covers
/// 1..NumberOfPicks -- no gaps, no duplicate slots, no empty position list.
/// Shared by CreateGuestDraftCommandHandler and UpdateGuestDraftCommandHandler's
/// ChangeType path. This was previously duplicated verbatim in both handlers by
/// deliberate choice (a little repetition over a shared utility that didn't
/// otherwise exist in this feature set); pulled out now because the duplicated
/// part was the actual Distinct()/SetEquals() coverage algorithm, not the
/// per-command DTO-to-tuple mapping -- a second copy of that specific check
/// silently drifting is a real risk, not a hypothetical one. Each handler still
/// does its own DTO-to-tuple projection locally, since CreateGuestDraftPositionInput
/// and UpdateGuestDraftPositionInput are distinct types with no shared base.
/// </summary>
internal static class GuestDraftPositionCoverage
{
  public static Result Validate(
    IReadOnlyList<IReadOnlyList<int>> positionPickSlots,
    int numberOfPicks
  )
  {
    ArgumentNullException.ThrowIfNull(positionPickSlots);

    if (positionPickSlots.Count == 0)
    {
      return Result.Failure(DraftErrors.PositionsAreRequiredForThisDraftType);
    }

    var allSlots = positionPickSlots.SelectMany(picks => picks).ToList();
    var expectedSlots = Enumerable.Range(1, numberOfPicks).ToHashSet();

    return allSlots.Count != allSlots.Distinct().Count() || !expectedSlots.SetEquals(allSlots)
      ? Result.Failure(DraftErrors.PositionsMustExactlyCoverTheNumberOfPicks)
      : Result.Success();
  }
}

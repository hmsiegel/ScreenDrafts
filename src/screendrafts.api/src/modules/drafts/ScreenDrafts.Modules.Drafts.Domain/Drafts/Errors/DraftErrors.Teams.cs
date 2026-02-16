using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static partial class DraftErrors
{
  public static readonly SDError TooManyDrafterTeams =
    SDError.Problem(
      "Drafts.TooManyDrafterTeams",
      "Cannot add more drafter teams than the total allowed.");

  public static SDError DrafterTeamAlreadyAdded(Guid id) =>
    SDError.Conflict(
      "Drafts.DrafterTeamAlreadyAdded",
      $"Drafter team with id {id} is already added to the draft.");

  public static SDError DrafterTeamContainsOverlappingDrafters(IEnumerable<Guid> ids) =>
    SDError.Conflict(
      "Drafts.DrafterTeamContainsOverlappingDrafters",
      $"Drafter team contains overlapping drafters: {ids}.");

  public static SDError TeamAlreadyAdded(Guid id) =>
    SDError.Conflict(
      "Drafts.TeamAlreadyAdded",
      $"Team with id {id} is already added to the draft.");

  public static readonly SDError DrafterTeamDoesNotBelongToThisDraft =
    SDError.Problem(
      "Drafts.DrafterTeamDoesNotBelongToThisDraft",
      "Drafter team does not belong to this draft.");
}

using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

public static class DrafterErrors
{
  public static readonly SDError RolloverVetoAlreadyExists =
    SDError.Conflict(
      "Drafters.RolloverVetoAlreadyExists",
      "A rollover veto already exists for this drafter.");

  public static readonly SDError RolloverVetoOverrideAlreadyExists =
    SDError.Conflict(
      "Drafters.RolloverVetoOverrideAlreadyExists",
      "A rollover veto override already exists for this drafter.");

  public static SDError NotFound(Guid? drafterId = null, Guid? drafterTeamId = null) =>
    SDError.NotFound(
      "Drafters.NotFound",
      $"Unable to find a drafter with Id {drafterId} or a drafter team with Id {drafterTeamId}.");

  public static SDError NotFound(Guid drafterId) =>
    SDError.NotFound(
      "Drafters.NotFound",
      $"Unable to find a drafter with Id {drafterId}.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "Drafters.NotFound",
      $"Unable to find a drafter with Id {publicId}.");

  public static SDError VetoNotFound(Guid vetoId) =>
    SDError.NotFound(
      "Drafters.VetoNotFound",
      $"Veto with ID {vetoId} not found.");

  public static readonly SDError CannotCreatDrafter =
    SDError.Failure(
      "Drafters.CannotCreatDrafter",
      "Cannot create drafter without user ID or name.");

  public static readonly SDError InvalidQuestionsWon =
    SDError.Failure(
      "Drafters.InvalidQuestionsWon",
      "Questions won must be greater than or equal to 0.");

  public static readonly SDError InvalidPosition =
    SDError.Failure(
      "Drafters.InvalidPosition",
      "Position must be greater than or equal to 0 and less than the total number of drafters.");

  public static SDError VetoOverrideNotFound(Guid vetoOverrideId) =>
    SDError.NotFound(
      "Drafters.VetoOverrideNotFound",
      $"Veto override with ID {vetoOverrideId} not found.");

  public static SDError AlreadyAdded(Guid drafterId) =>
    SDError.Conflict(
      "Drafters.AlreadyAdded",
      $"Drafter with ID {drafterId} already added to the team.");

  public static SDError AlreadyExistsForPerson(string personId) =>
    SDError.Conflict(
      "Drafters.AlreadyExistsForPerson",
      $"A drafter already exists for person with ID {personId}.");


  public static readonly SDError NotEnoughDrafters =
    SDError.Failure(
      "Drafters.NotEnoughDrafters",
      "There must be at least two drafters on a team.");

  public static readonly SDError InvalidBlessingRequest =
    SDError.Failure(
      "Drafters.InvalidBlessingRequest",
      "Invalid blessing request. You must specity either a drafter or a drafter team, but not both.");

  public static readonly SDError AlreadyRetired =
    SDError.Conflict(
      "Drafters.AlreadyRetired",
      "The drafter is already retired.");
}

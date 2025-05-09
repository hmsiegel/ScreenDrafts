﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;

public static class DraftErrors
{
  public static SDError NotFound(Guid draftId) =>
    SDError.NotFound(
      "Drafts.NotFound",
      $"Draft with id {draftId} was not found.");

  public static readonly SDError TooManyDrafters =
    SDError.Problem(
      "Drafts.TooManyDrafters",
      "Cannot add more drafters than the total allowed.");

  public static SDError DrafterAlreadyAdded(Guid drafterId) =>
    SDError.Conflict(
      "Drafts.DrafterAlreadyAdded",
      $"Drafter with id {drafterId} is already added to the draft.");

  public static SDError PickPositionAlreadyTaken(int position) =>
   SDError.Conflict(
     "Drafts.PickPositionAlreadyTaken",
     $"Pick position {position} is already taken.");

  public static SDError HostAlreadyAdded(Guid hostId) =>
    SDError.Conflict(
      "Drafts.HostAlreadyTaken",
      $"Host with id {hostId} is already added to the draft.");

  public static readonly SDError DraftNotStarted =
    SDError.Problem(
      "Drafts.DraftNotStarted",
      "Cannot add picks unless the draft has started.");

  public static readonly SDError TooManyHosts =
    SDError.Conflict(
      "Drafts.TooManyHosts",
      "Cannot add more hosts than the total allowed.");

  public static readonly SDError CannotVetoUnlessTheDraftIsStarted =
    SDError.Problem(
      "Drafts.CannotVetoUnlessTheDraftIsStarted",
      "Cannot veto a pick unless the draft has started.");

  public static readonly SDError CannotVetoAPickThatDoesNotExist =
    SDError.Problem(
      "Drafts.CannotVetoAPickThatDoesNotExist",
      "Cannot veto a pick that does not exist.");

  public static readonly SDError CannotVetoAPickThatIsNotYours =
    SDError.Conflict(
      "Drafts.CannotVetoAPickThatIsNotYours",
      "Cannot veto a pick that is not yours.");

  public static readonly SDError CannotVetoAPickThatIsAlreadyVetoed =
    SDError.Conflict(
      "Drafts.CannotVetoAPickThatIsAlreadyVetoed",
      "Cannot veto a pick that is already vetoed.");

  public static readonly SDError OnlyDraftersInTheDraftCanUseAVeto =
    SDError.Problem(
      "Drafts.OnlyDraftersInTheDraftCanUseAVeto",
      "Only drafters in the draft can use a veto.");

  public static readonly SDError DraftCanOnlyBeStartedIfItIsCreated =
    SDError.Problem(
      "Drafts.DraftCanOnlyBeStartedIfItIsCreated",
      "Draft can only be started if it is created.");

  public static readonly SDError CannotStartDraftWithoutAllDrafters =
    SDError.Problem(
      "Drafts.CannotStartDraftWithoutAllDrafters",
      "Cannot start the draft without all drafters.");

  public static readonly SDError CannotStartDraftWithoutAllHosts =
    SDError.Problem(
      "Drafts.CannotStartDraftWithoutAllHosts",
      "Cannot start the draft without all hosts.");

  public static readonly SDError CannotCompleteDraftIfItIsNotInProgress =
    SDError.Problem(
      "Drafts.CannotCompleteDraftIfItIsNotInProgress",
      "Cannot readonly complete the draft if it is not in progress.");

  public static readonly SDError CannotCompleteDraftWithoutAllPicks =
    SDError.Problem(
      "Drafts.CannotCompleteDraftWithoutAllPicks",
      "Cannot readonly complete the draft without all picks.");

  public static readonly SDError CannotVetoOverrideAVetoThatDoesNotExist =
    SDError.Problem(
      "Drafts.CannotVetoOverrideAVetoThatDoesNotExist",
      "Cannot readonly veto override a veto that does not exist.");

  public static readonly SDError OnlyDraftersInTheDraftCanUseAVetoOverride =
    SDError.Problem(
      "Drafts.OnlyDraftersInTheDraftCanUseAVetoOverride",
      "Only drafters in the draft can use a veto override.");

  public static readonly SDError ADrafterCanOnlyHaveOneRolloverVeto =
    SDError.Problem(
      "Drafts.ADrafterCanOnlyHaveOneRolloverVeto",
      "A drafter can only have one rollover veto.");

  public static readonly SDError ADrafterCanOnlyHaveOneRolloverVetoOverride =
    SDError.Problem(
      "Drafts.ADrafterCanOnlyHaveOneRolloverVetoOverride",
      "A drafter can only have one rollover veto override.");

  public static readonly SDError DraftMustHaveAtLeastTwoParticipants = SDError.Problem(
    "Drafts.DraftMustHaveAtLeastTwoParticipants",
    "Draft must have at least two participants (drafters or drafter teams).");

  public static readonly SDError PickPositionIsOutOfRange = SDError.Conflict(
    "Drafts.PickPositionIsOutOfRange",
    "Pick position is out of range.");

  public static readonly SDError DraftMustHaveAtLeastFivePicks = SDError.Problem(
    "Drafts.DraftMustHaveAtLeastFivePicks",
    "Draft must have at least five picks.");

  public static readonly SDError CannotPauseDraftIfItIsNotInProgress = SDError.Problem(
    "Drafts.CannotPauseDraftIfItIsNotInProgress",
    "Cannot pause the draft if it is not in progress.");

  public static readonly SDError CannotResumeDraftIfItIsNotPaused = SDError.Problem(
    "Drafts.CannotResumeDraftIfItIsNotPaused",
    "Cannot resume the draft if it is not paused.");

  public static readonly SDError CannotPauseDraftIfItIsAlreadyPaused = SDError.Problem(
    "Drafts.CannotPauseDraftIfItIsAlreadyPaused",
    "Cannot pause the draft if it is already paused.");

  public static readonly SDError CannotResumeDraftIfItIsAlreadyInProgress = SDError.Problem(
    "Drafts.CannotResumeDraftIfItIsAlreadyInProgress",
    "Cannot resume the draft if it is already in progress.");

  public static readonly SDError CannotPauseDraftIfItIsCompleted = SDError.Problem(
    "Drafts.CannotPauseDraftIfItIsCompleted",
    "Cannot pause the draft if it is completed.");

  public static readonly SDError CannotResumeDraftIfItIsCompleted = SDError.Problem(
    "Drafts.CannotResumeDraftIfItIsCompleted",
    "Cannot resume the draft if it is completed.");

  public static readonly SDError InvalidPickPosition = SDError.Problem(
    "Drafts.InvalidPickPosition",
    "Invalid pick position.");

  public static SDError MovieNotFound(Guid movieId) =>
    SDError.NotFound(
      "Drafts.MovieNotFound",
      $"Movie with id {movieId} was not found.");

  public static SDError MovieAlreadyAdded(Guid movieId) =>
    SDError.Conflict(
      "Drafts.MovieAlreadyAdded",
      $"Movie with id {movieId} is already added to the draft.");

  public static SDError MovieAlreadyPicked(Guid movieId) =>
    SDError.Conflict(
      "Drafts.MovieAlreadyPicked",
      $"Movie with id {movieId} is already picked.");

  public static SDError InvalidNumberOfPicks(int totalPicks, int numberOfDraftPositionPicks) =>
    SDError.Problem(
      "Drafts.InvalidNumberOfPicks",
      $"Total picks {totalPicks} does not match the number of picks {numberOfDraftPositionPicks}.");

  public static readonly SDError CannotAddPickIfDraftIsNotStarted = SDError.Problem(
    "Drafts.CannotAddPickIfDraftIsNotStarted",
    "Cannot add a pick if the draft is not started.");

  public static readonly SDError CannotAddPickIfDraftIsCompleted = SDError.Problem(
    "Drafts.CannotAddPickIfDraftIsCompleted",
    "Cannot add a pick if the draft is completed.");

  public static readonly SDError CannotAddPickIfDraftIsPaused = SDError.Problem(
    "Drafts.CannotAddPickIfDraftIsPaused",
    "Cannot add a pick if the draft is paused.");

  public static readonly SDError CannotAddTriviaResultIfDraftIsNotStarted = SDError.Problem(
    "Drafts.CannotAddTriviaResultIfDraftIsNotStarted",
    "Cannot add a trivia result if the draft is not started.");

  public static readonly SDError PicksNotFound = SDError.NotFound(
    "Drafts.PicksNotFound",
    "No draft picks were found.");

  public static SDError MovieAlreadyExists(string imdbId) =>
    SDError.Conflict(
      "Drafts.MovieAlreadyExists",
      $"Movie with IMDB id {imdbId} already exists.");

  public static SDError PickNotFound(Guid id) =>
    SDError.NotFound(
      "Drafts.PickNotFound",
      $"Pick with id {id} was not found.");

  public static readonly SDError CommissionerOverridesNotFound =
    SDError.NotFound(
      "Drafts.CommissionerOverridesNotFound",
      "No commissioner overrides were found.");

  public static readonly SDError CommissionerOverrideCannotBeApplied =
    SDError.Problem(
      "Drafts.CommissionerOverrideCannotBeApplied",
      "Commissioner override cannot be applied.");

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

  public static SDError AlreadyAdded(Guid id) =>
    SDError.Conflict(
      "Drafts.AlreadyAdded",
      $"Team with id {id} is already added to the draft.");

  public static readonly SDError CannotAddTriviaResultWithoutDrafterOrDrafterTeam =
    SDError.Problem(
      "Drafts.CannotAddTriviaResultWithoutDrafterOrDrafterTeam",
      "Cannot add a trivia result without a drafter or drafter team.");
}

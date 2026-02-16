using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Errors;
public static partial class DraftErrors
{
  public static SDError NotFound(Guid draftId) =>
    SDError.NotFound(
      "Drafts.NotFound",
      $"Draft with id {draftId} was not found.");

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      "Drafts.NotFound",
      $"Draft with public id {publicId} was not found.");

  public static readonly SDError DraftNotStarted =
    SDError.Problem(
      "Drafts.DraftNotStarted",
      "Cannot add picks unless the draft has started.");

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
      "Cannot complete the draft if it is not in progress.");

  public static readonly SDError CannotCompleteDraftWithoutAllPicks =
    SDError.Problem(
      "Drafts.CannotCompleteDraftWithoutAllPicks",
      "Cannot complete the draft without all picks.");

  public static readonly SDError DraftMustHaveAtLeastTwoParticipants = SDError.Problem(
    "Drafts.DraftMustHaveAtLeastTwoParticipants",
    "Draft must have at least two participants (drafters or drafter teams).");

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



  public static readonly SDError CommissionerOverridesNotFound =
    SDError.NotFound(
      "Drafts.CommissionerOverridesNotFound",
      "No commissioner overrides were found.");

  public static readonly SDError CommissionerOverrideCannotBeApplied =
    SDError.Problem(
      "Drafts.CommissionerOverrideCannotBeApplied",
      "Commissioner override cannot be applied.");

  public static readonly SDError CannotContinueDraftIfItIsNotPaused =
    SDError.Problem(
      "Drafts.CannotContinueDraftIfItIsNotPaused",
      "Cannot continue the draft if it is not paused.");

  public static readonly SDError CannotEditADraftAfterItHasBeenStarted = SDError.Problem(
    "Drafts.CannotEditADraftAfterItHasBeenStarted",
    "Cannot edit a draft after it has been started.");


  public static readonly SDError DraftIsRequired = SDError.Problem(
    "Drafts.DraftIsRequired",
    "Draft is required.");

  public static readonly SDError ReleaseDateIsRequired = SDError.Problem(
    "Drafts.ReleaseDateIsRequired",
    "Release date is required.");

  public static SDError SeriesAlreadyLinked(Guid seriesId) =>
    SDError.Conflict(
      "Drafts.SeriesAlreadyLinked",
      $"Series with id {seriesId} is already linked to the draft.");

  public static SDError MovieAlreadyPickedInDraft(Movie movie)
  {
    ArgumentNullException.ThrowIfNull(movie);

    return SDError.Problem(
      "Drafts.MovieAlreadyPickedInDraft",
      $"Movie '{movie.MovieTitle}' (ID: {movie.Id}) has already been picked in this draft.");
  }

  public static readonly SDError EpisodeNumberMustBeGreaterThanZero =
    SDError.Problem(
      "Drafts.EpisodeNumberMustBeGreaterThanZero",
      "Episode number must be greater than zero.");

  public static readonly SDError NoSeriesLinked = 
    SDError.Problem(
      "Drafts.NoSeriesLinked",
      "No series is linked to the draft.");

  public static readonly SDError CommunityPicksNotAllowedInThisDraftPart =
    SDError.Problem(
      "Drafts.CommunityPicksNotAllowedInThisDraftPart",
      "Community picks are not allowed in this draft part.");

  public static readonly SDError NoRemainingCommunityPicks =
    SDError.Problem(
      "Drafts.NoRemainingCommunityPicks",
      "No remaining community picks are available in this draft part.");

  public static readonly SDError NoRemainingVetoes =
    SDError.Problem(
      "Drafts.NoRemainingCommunityPicks",
      "No remaining community picks are available in this draft part.");

  public static readonly SDError NoScheduledDraftPartToContinue =
    SDError.Problem(
      "Drafts.NoScheduledDraftPartToContinue",
      "There is no scheduled draft part to continue at this time.");

  public static readonly SDError InvalidDraftPartStatusAction =
    SDError.Problem(
      "Drafts.InvalidDraftPartStatusAction",
      "The requested draft part status action is invalid.");

  public static readonly SDError CannotChangeASeriesAfterADraftPartHasStarted =
    SDError.Conflict(
      "Drafts.CannotChangeASeriesAfterADraftPartHasStarted",
      "Cannot change a series after a draft part has started.");

  public static readonly SDError CannotChangeDraftTypeAfterADraftPartHasStarted = SDError.Conflict(
      "Drafts.CannotChangeDraftTypeAfterADraftPartHasStarted",
      "Cannot change a draft type after a draft part has started.");

  public static readonly SDError SeriesIdIsRequired = 
    SDError.Problem(
      "Drafts.SeriesIdIsRequired",
      "Series id is required.");

  public static readonly SDError TotalPicksIsOutOfRange =
    SDError.Problem(
      "Drafts.TotalPicksIsOutOfRange",
      "Total picks is out of range.");

  public static readonly SDError DraftTypeIsRequired = SDError.Problem(
      "Drafts.DraftTypeIsRequired",
      "Draft type is required.");

  public static readonly SDError SeriesIsRequired = 
        SDError.Problem(
      "Drafts.SeriesIsRequired",
      "Series is required.");

  public static SDError CampaignDoesNotBelongToThisDraft(string publicId) =>
    SDError.Problem(
      "Drafts.CampaignDoesNotBelongToThisDraft",
      $"Campaign with public id {publicId} does not belong to this draft.");

  public static SDError DraftPartNotFoundByIndex(string draftPublicId, int partIndex) =>
    SDError.NotFound(
      "Drafts.DraftPartNotFoundByIndex",
      $"Draft part with index {partIndex} was not found in draft with public id {draftPublicId}.");

  public static SDError CannotUpdateCompletedOrCancelledDraft(string publicId) =>
        SDError.Conflict(
          "Drafts.CannotUpdateCompletedOrCancelledDraft",
          $"Cannot update draft with public id {publicId} because it is completed or cancelled.");
}

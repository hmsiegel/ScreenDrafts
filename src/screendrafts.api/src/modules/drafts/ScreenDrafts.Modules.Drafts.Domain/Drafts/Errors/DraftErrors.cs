
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

  public static readonly SDError SeriesNameIsRequired = SDError.Problem(
    "Drafts.SeriesNameIsRequired",
    "Series name is required.");

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

  public static SDError SeriesNotFound(Guid seriesId) =>
    SDError.NotFound(
      "Drafts.SeriesNotFound",
      $"Series with id {seriesId} was not found.");

  public static SDError CampaignNotFound(Guid campaignId) =>
    SDError.NotFound(
      "Drafts.CampaignNotFound",
      $"Campaign with id {campaignId} was not found.");

  public static SDError CampaignNotFound(string publicId) =>
    SDError.NotFound(
      "Drafts.CampaignNotFound",
      $"Campaign with public id {publicId} was not found.");

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

  public static SDError CampaignDoesNotBelongToThisDraft(string publicId) =>
    SDError.Problem(
      "Drafts.CampaignDoesNotBelongToThisDraft",
      $"Campaign with public id {publicId} does not belong to this draft.");
}

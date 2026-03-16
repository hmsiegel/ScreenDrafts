namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class CandidateListErrors
{
  public static readonly SDError EntryAlreadyResolved = SDError.Conflict(
    "CandidateListErrors.EntryAlreadyResolved",
    "Candidate list entry is not pending.");

  public static SDError DraftPartNotFound(string draftPartId) => SDError.NotFound(
    "CandidateListErrors.DraftPartNotFound",
    $"Draft part with ID '{draftPartId}' was not found.");

  public static SDError InvalidTmdbId(int tmdbId) => SDError.Problem(
    "CandidateListErrors.InvalidTmdbId",
    $"The TMDB ID '{tmdbId}' is invalid.");

  public static SDError EntryNotFound(int tmdbId) => SDError.NotFound(
    "CandidateListErrors.EntryNotFound",
    $"Candidate list entry with TMDB ID '{tmdbId}' was not found.");
}

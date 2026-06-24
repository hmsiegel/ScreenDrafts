namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Errors;

public static class CommunityFilmRuleErrors
{
  public static readonly SDError TargetSlotRequired = SDError.Failure(
    code: "CommunityFilmRule.TargetSlotRequired",
    description: "A target slot is required for a Boosters Pick rule."
  );

  public static readonly SDError NotModifiable = SDError.Conflict(
    code: "CommunityFilmRule.NotModifiable",
    description: "Community film rules can only be modified when the draft part is Created or Paused."
  );

  public static readonly SDError CommunityParticipantNotFound = SDError.Failure(
    code: "CommunityFilmRule.CommunityParticipantNotFound",
    description: "The community participant (Patrion members) must be added to this draft part before creating a community film rule."
  );

  public static SDError Duplicate(int tmdbId) =>
    SDError.Conflict(
      code: "CommunityFilmRule.Duplicate",
      description: $"A community film rule for TMDb ID '{tmdbId}' already exists on this draft part."
    );

  public static SDError NotFound(string publicId) =>
    SDError.NotFound(
      code: "CommunityFilmRule.NotFound",
      description: $"No community film rule for Public ID '{publicId}' exists on this draft part."
    );

  internal static SDError FilmAlreadyAssigned(int tmdbId) =>
    SDError.Conflict(
      code: "CommunityFilmRule.FilmAlreadyAssigned",
      description: $"The film with TMDb ID '{tmdbId}' is already assigned to another community film rule on this draft part."
    );
}

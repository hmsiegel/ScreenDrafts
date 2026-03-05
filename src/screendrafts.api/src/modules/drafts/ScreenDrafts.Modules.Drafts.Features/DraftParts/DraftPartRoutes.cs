namespace ScreenDrafts.Modules.Drafts.Features.DraftParts;

internal static class DraftPartRoutes
{
  // Core
  public const string Base = "/drafts/{draftId}/parts";
  public const string ById = "/draft-parts/{draftPartId}";

  // Status
  public const string Status = "/draft-parts/{draftPartId}/status";

  // Releases
  public const string Releases = "/draft-parts/{draftPartId}/releases";
  public const string RemoveReleaseDate = "/draft-parts/{draftPartId}/releases/{releaseId}";
  public const string Episode = "/draft-parts/{draftPartId}/episode";

  // Hosts
  public const string Hosts = "/draft-parts/{draftPartId}/hosts";
  public const string RemoveHost = "/draft-parts/{draftPartId}/hosts/{hostId}";
  public const string SetPrimaryHost = "/draft-parts/{draftPartId}/hosts/{hostId}/primary";

  // Participants
  public const string Participants = "/draft-parts/{draftPartId}/participants";
  public const string SetCommunityParticipant = "/draft-parts/{draftPartId}/participants/community";
  public const string RemoveCommunityParticipant = "/draft-parts/{draftPartId}/participants/community/{communityId}";
  public const string SetDraftPositions = "/draft-parts/{draftPartId}/positions";
  public const string AssignParticipantToDraftPosition = "/draft-parts/{draftPartId}/positions/{positionId}/participant";
  public const string AssignTriviaResults = "/draft-parts/{draftPartId}/trivia-results";

  // Picks
  public const string Picks = "/draft-parts/{draftPartId}/picks";
  public const string UndoPick = "/draft-parts/{draftPartId}/picks/{playOrder}";

  // Vetos and Veto Overrides
  public const string ApplyVeto = "/draft-parts/{draftPartId}/picks/{playOrder}/veto";
  public const string ApplyVetoOverride = "/draft-parts/{draftPartId}/veto-override/{pickId}";
  public const string ApplyCommissionerOverride = "/draft-parts/{draftPartId}/commissioner-override/{pickId}";
}


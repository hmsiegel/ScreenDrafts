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
  public const string AddDrafterToDraftPart = "/draft-parts/{draftPartId}/drafters";
  public const string RemoveDrafterFromDraftPart = "/draft-parts/{draftPartId}/drafters/{drafterId}";
  public const string AddDrafterTeamToDraftPart = "/draft-parts/{draftPartId}/drafter-teams";
  public const string RemoveDrafterTeamFromDraftPart = "/draft-parts/{draftPartId}/drafter-teams/{drafterTeamId}";
  public const string SetCommunityParticipant = "/draft-parts/{draftPartId}/participants/community";
  public const string RemoveCommunityParticipant = "/draft-parts/{draftPartId}/participants/community/{communityId}";
  public const string SetDraftPositions = "/draft-parts/{draftPartId}/positions";

  // Picks
  public const string AddPick = "/draft-parts/{draftPartId}/picks";

  // Vetos and Veto Overrides
  public const string ApplyVeto = "/draft-parts/{draftPartId}/veto";
  public const string ApplyVetoOverride = "/draft-parts/{draftPartId}/veto-override";
  public const string ApplyCommissionerOverride = "/draft-parts/{draftPartId}/commissioner-override";
}

